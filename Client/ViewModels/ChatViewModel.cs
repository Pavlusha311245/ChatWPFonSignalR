using Client.Commands;
using Client.Models;
using Client.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Media;

namespace Client
{
    class ChatViewModel : ViewModelBase
    {
        HubConnection hubConnection;
        public Chat ReceivingChat { get; set; }
        public Server.Models.SenderMessage SenderMessage { get; set; } = new();

        public ObservableCollection<Chat> Chats { get; }
        public ObservableCollection<ReceivedMessage> Messages { get; }
        public ObservableCollection<Models.Task> Tasks { get; }

        public NotificationManager NotificationManager { get; set; }

        public string AppUrlString { get; set; } = @"https://localhost:44316";

        System.Windows.Visibility isAdmin;

        public System.Windows.Visibility IsAdmin
        {
            get => isAdmin;
            set
            {
                if (isAdmin != value)
                {
                    isAdmin = value;
                    OnPropertyChanged("IsAdmin");
                }
            }
        }

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

        public RelayCommand SendMessageCommand { get; }
        public RelayCommand SendTaskCommand { get; }

        /// <summary>
        /// ViewModel constructor
        /// </summary>
        /// <param name="accessToken"></param>
        public ChatViewModel(string accessToken)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(AppUrlString + "/chat", options =>
                {
                    options.AccessTokenProvider = () => System.Threading.Tasks.Task.FromResult(accessToken);
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            Messages = new ObservableCollection<ReceivedMessage>();
            Chats = new ObservableCollection<Chat>();
            Tasks = new ObservableCollection<Models.Task>();

            IsConnected = false;
            IsBusy = false;
            IsAdmin = System.Windows.Visibility.Hidden;

            SendMessageCommand = new RelayCommand(async o =>
            {
                SenderMessage.Documents = null;
                SenderMessage.Task = null;
                await SendToUsers();
            }, o => IsConnected);
            SendTaskCommand = new RelayCommand(async o => await SendToUsers(), o => IsConnected);

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

            hubConnection.On("AdminPrivileges", () =>
            {
                IsAdmin = System.Windows.Visibility.Visible;
            });

            hubConnection.On<List<Task>, List<Chat>>("Connected", (tasks, chats) =>
            {
                foreach (var chat in chats)
                    if (!Chats.Contains(chat))
                    {
                        Chats.Insert(0, chat);
                    }

                foreach (var task in tasks)
                {
                    Tasks.Insert(0, task);
                }

                NotificationManager.Show("Информация", "Соединение с сервером установлено", NotificationType.Information);
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

                SenderMessage.Task = null;
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
            Messages.Insert(0, new ReceivedMessage
            {
                Message = message,
                User = user
            });
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
                await hubConnection.InvokeAsync("SendMessage", SenderMessage);
                SenderMessage.MessageText = string.Empty;
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
