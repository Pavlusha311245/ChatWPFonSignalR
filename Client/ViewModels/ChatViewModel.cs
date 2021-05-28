﻿using Client.Commands;
using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Client
{
    class ChatViewModel : INotifyPropertyChanged
    {
        HubConnection hubConnection;
        HttpWebRequest httpRequest;

        public Server.Models.Message MessageTask { get; set; }

        public ObservableCollection<ListBoxItem> Users { get; set; }
        public ObservableCollection<MessageData> Messages { get; }

        public NotificationManager NotificationManager { get; set; }

        public string AppUrlString { get; set; } = @"https://localhost:44316";

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }

        bool isConnected;
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        public SendMessageCommand SendMessageCommand { get; }

        public ChatViewModel(string accessToken)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(AppUrlString + "/chat", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            httpRequest = WebRequest.CreateHttp(AppUrlString + "/api/Users");
            httpRequest.PreAuthenticate = true;
            httpRequest.Headers.Add("Authorization", "Bearer " + accessToken);
            httpRequest.ContentType = "application/json";

            Messages = new ObservableCollection<MessageData>();
            Users = new ObservableCollection<ListBoxItem>();

            IsConnected = false;
            IsBusy = false;

            SendMessageCommand = new SendMessageCommand(async o => await SendMessageToEveryOne(), o => IsConnected);

            NotificationManager = new NotificationManager();

            hubConnection.Closed += async (error) =>
            {
                IsConnected = false;
                await Task.Delay(3000);
                await Connect();
            };

            hubConnection.On<string, string>("Receive", (user, message) =>
                    {
                        SendDataToMessageListView(user, message);
                    });

            hubConnection.On<string>("Connected", (message) =>
            {
                NotificationManager.Show("Информация", message, NotificationType.Information);
                SystemSounds.Exclamation.Play();
                IsBusy = true;
                IsConnected = true;
            });
        }

        /// <summary>
        /// Executing connections to HUB
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            if (IsConnected)
                return;

            try
            {
                await hubConnection.StartAsync();

                var response = (HttpWebResponse)(await httpRequest.GetResponseAsync());
                var stream = response.GetResponseStream();

                string jsonString;
                using (StreamReader streamReader = new(stream))
                    jsonString = await streamReader.ReadToEndAsync();

                var users = JsonConvert.DeserializeObject<IEnumerable<User>>(jsonString);

                foreach (var user in users)
                {
                    ListBoxItem listBoxItem = new();
                    listBoxItem.Content = user.Email;
                    Users.Insert(0, listBoxItem);
                }

                MessageTask.Task = null;
                IsConnected = true;
            }
            catch (Exception ex)
            {
                NotificationManager.Show("Предупреждение", ex.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Executing disconnect to HUB
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            if (!IsConnected)
                return;

            await hubConnection.StopAsync();

            IsConnected = false;
            SendDataToMessageListView(string.Empty, "Вы были отключены от чата");
        }

        /// <summary>
        /// Send MessageData to WPF application
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        private void SendDataToMessageListView(string user, string message)
        {
            Messages.Insert(0, new MessageData
            {
                Message = message,
                User = user
            });
        }

        /// <summary>
        /// Sending asynchronous message to HUB 
        /// </summary>
        /// <returns></returns>
        async Task SendMessageToEveryOne()
        {
            try
            {
                IsBusy = true;
                await hubConnection.InvokeAsync("SendToEveryone", MessageTask);
                MessageTask.Task = null;
            }
            catch (Exception ex)
            {
                SendDataToMessageListView(string.Empty, $"Критическая ошибка: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
