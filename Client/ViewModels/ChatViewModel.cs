using Client.Commands;
using Client.Models;
using Client.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Media;
using System.Net;
using System.Threading.Tasks;

namespace Client
{
    class ChatViewModel : ViewModelBase
    {
        HubConnection hubConnection;
        string token;
        public List<string> Receivers { get; set; } = new();

        public User User { get; set; }

        public Server.Models.Message MessageTask { get; set; }

        public ObservableCollection<User> Users { get; }
        public ObservableCollection<MessageData> Messages { get; }
        public ObservableCollection<Models.Task> Tasks { get; }

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

        /// <summary>
        /// ViewModel constructor
        /// </summary>
        /// <param name="accessToken"></param>
        public ChatViewModel(string accessToken)
        {
            token = accessToken;
            hubConnection = new HubConnectionBuilder()
                .WithUrl(AppUrlString + "/chat", options =>
                {
                    options.AccessTokenProvider = () => System.Threading.Tasks.Task.FromResult(token);
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            Messages = new ObservableCollection<MessageData>();
            Users = new ObservableCollection<User>();
            Tasks = new ObservableCollection<Models.Task>();

            IsConnected = false;
            IsBusy = false;

            SendMessageCommand = new SendMessageCommand(async o => await SendToUsers(), o => IsConnected);

            NotificationManager = new NotificationManager();

            //HUB Connection commands
            hubConnection.Closed += async (error) =>
            {
                IsConnected = false;
                await System.Threading.Tasks.Task.Delay(3000);
                await Connect();
            };

            hubConnection.On<string, string>("Receive", (user, message) =>
                    {
                        SendDataToMessageListView(user, message);
                    });

            hubConnection.On<string, List<User>>("Connected", (message, users) =>
            {
                foreach (var user in users)
                    if (!(user.Id == User.Id))
                        Users.Insert(0, new User
                        {
                            Email = user.Email
                        });

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
        public async System.Threading.Tasks.Task Connect()
        {
            if (IsConnected)
                return;

            try
            {
                await hubConnection.StartAsync();

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
        public async System.Threading.Tasks.Task Disconnect()
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
        async System.Threading.Tasks.Task SendMessageToEveryOne()
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

        /// <summary>
        /// Sending asynchronous message to HUB 
        /// </summary>
        /// <returns></returns>
        async System.Threading.Tasks.Task SendToUsers()
        {
            try
            {
                IsBusy = true;
                await hubConnection.InvokeAsync("SendToUsers", MessageTask, Receivers);
                MessageTask.Task = null;
                Receivers.Clear();
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
    }
}
