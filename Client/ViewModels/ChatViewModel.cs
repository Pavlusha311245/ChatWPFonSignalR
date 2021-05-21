using Client.Commands;
using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Client
{
    class ChatViewModel : INotifyPropertyChanged
    {
        HubConnection hubConnection;

        public string Message { get; set; }
        public byte[] File { get; set; }

        public ObservableCollection<MessageData> Messages { get; }

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
                .WithUrl("https://localhost:44316/chat", options =>
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

            Messages = new ObservableCollection<MessageData>();

            IsConnected = false;
            IsBusy = false;

            SendMessageCommand = new SendMessageCommand(async o => await SendMessage(), o => IsConnected);

            hubConnection.Closed += async (error) =>
            {
                SendLocalMessage(string.Empty, "Соединение закрыто");
                IsConnected = false;
                await Task.Delay(5000);
                await Connect();
            };

            hubConnection.On<string, string>("Receive", (user, message) =>
                    {
                        SendLocalMessage(user, message);
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
                SendLocalMessage(string.Empty, "Подключён к чату");

                IsConnected = true;
            }
            catch (Exception ex)
            {
                SendLocalMessage(string.Empty, $"Ошибка соединения: {ex.Message}");
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
            SendLocalMessage(string.Empty, "Вы были отключены от чата");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        private void SendLocalMessage(string user, string message)
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
        async Task SendMessage()
        {
            try
            {
                IsBusy = true;
                await hubConnection.InvokeAsync("Send", Message, File);
                File = null;
            }
            catch (Exception ex)
            {
                SendLocalMessage(string.Empty, $"Критическая ошибка: {ex.Message}");
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
