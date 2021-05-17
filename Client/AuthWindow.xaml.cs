using Client.Data;
using Client.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
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
            MailAddress mail = null;

            if (!MailAddress.TryCreate(LoginEmailBox.Text.Trim(), out mail))
            {
                MessageBox.Show("Invalid email");
                return;
            }               

            if (string.IsNullOrWhiteSpace(mail.Address) || string.IsNullOrWhiteSpace(LoginPassBox.Password.Trim()))
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

                if (deserializedResponse.IsSuccess)
                {
                    using (var db = new UserContext())
                    {
                        Token token = new()
                        {
                            Value = deserializedResponse.Message,
                            ExpireDate = (System.DateTime)deserializedResponse.ExpireDate
                        };
                        db.Tokens.Add(token);
                        db.SaveChanges();
                    }

                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
