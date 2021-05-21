using Client.Data;
using Client.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
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
            MailAddress mail;
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

            var creditals = new
            {
                Email = LoginEmailBox.Text,
                Password = LoginPassBox.Password
            };
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(App_Path + "/Auth/Login", creditals).Result;

                var deserializedResponse = response.Content.ReadFromJsonAsync<UserManagerResponse>().Result;

                if (deserializedResponse.IsSuccess)
                {
                    using (var db = new UserContext())
                    {
                        var user = JsonConvert.DeserializeObject<User>(((JsonElement)deserializedResponse.Model).GetRawText());

                        if (db.Users.Find(user.Id) == null)
                            db.Users.Add(user);

                        var token = db.Tokens.FirstOrDefault(token => token.User == user);
                        if (token == null)
                        {
                            db.Tokens.Add(new()
                            {
                                Value = deserializedResponse.Message,
                                ExpireDate = (System.DateTime)deserializedResponse.ExpireDate,
                                UserId = user.Id
                            });
                        }
                        else
                        {
                            token.Value = deserializedResponse.Message;
                            token.ExpireDate = (System.DateTime)deserializedResponse.ExpireDate;
                        }

                        db.SaveChanges();
                    }

                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
