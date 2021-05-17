using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client
{
    class ChatViewModel : INotifyPropertyChanged
    {
        HubConnection hubConnection;

        public string UserName { get; set; }
        public string Message { get; set; }

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

        public Command SendMessageCommand { get; }

        public ChatViewModel()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44316/chat")
                .Build();

            Messages = new ObservableCollection<MessageData>();

            IsConnected = false;
            IsBusy = false;

            SendMessageCommand = new Command(async o => await SendMessage(), o => IsConnected);

            hubConnection.Closed += async (error) =>
            {
                SendLocalMessage(string.Empty, "Connection closed");
                IsConnected = false;
                await Task.Delay(5000);
                await Connect();
            };

            hubConnection.On<string, string>("Receive", (user, message) =>
                    {
                        SendLocalMessage(user, message);
                    });
        }

        public async Task Connect()
        {
            if (IsConnected)
                return;

            try
            {
                await hubConnection.StartAsync();
                SendLocalMessage(string.Empty, "Connected in chat");

                IsConnected = true;
            }
            catch (Exception ex)
            {
                SendLocalMessage(string.Empty, $"Connection error: {ex.Message}");
            }
        }

        public async Task Disconnect()
        {
            if (!IsConnected)
                return;

            await hubConnection.StopAsync();

            IsConnected = false;
            SendLocalMessage(string.Empty, "You was disconnect from chat");
        }

        private void SendLocalMessage(string user, string message)
        {
            Messages.Insert(0, new MessageData
            {
                Message = message,
                User = user
            });
        }

        async Task SendMessage()
        {
            try
            {
                IsBusy = true;
                await hubConnection.InvokeAsync("Send", UserName, Message);
            }
            catch (Exception ex)
            {
                SendLocalMessage(string.Empty, $"Exception error: {ex.Message}");
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

    public class Command : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }

            remove { CommandManager.RequerySuggested -= value; }
        }

        public Command(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
