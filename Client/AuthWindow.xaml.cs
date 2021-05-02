using Client.Data;
using Client.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        const string App_Path = @"https://localhost:44316/api";

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginEmailBox.Text.Trim()) || string.IsNullOrWhiteSpace(LoginPassBox.Password.Trim()))
            {
                MessageBox.Show("Одно или несколько полей пустые");
                return;
            };

            var user = new
            {
                Email = LoginEmailBox.Text,
                Password = LoginPassBox.Password
            };
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(App_Path + "/Auth/Login", user).Result;

                var deserializedResponse = response.Content.ReadFromJsonAsync<UserManagerResponse>().Result;

                MainWindow mainWindow = new();

                if (deserializedResponse.IsSuccess)
                {
                    using (var db = new UserContext())
                    {
                        Token token = new()
                        {
                            Value = deserializedResponse.Message,
                            ExpireDate = (System.DateTime)deserializedResponse.ExpireDate
                        };
                        db.Token.Add(token);
                        db.SaveChanges();
                    }
                    mainWindow.Show();
                }

            }
        }
    }
}
