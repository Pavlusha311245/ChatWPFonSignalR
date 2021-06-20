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
        public string AccessToken { get; set; }

        public AuthWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Login user in app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Login(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginEmailBox.Text))
            {
                MessageBox.Show("Поле Email пустое");
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginPassBox.Password))
            {
                MessageBox.Show("Поле Password пустое");
                return;
            }
            MailAddress mail;
            if (!MailAddress.TryCreate(LoginEmailBox.Text.Trim(), out mail))
            {
                MessageBox.Show("Некорректный формат Email");
                return;
            }

            var creditals = new
            {
                Email = LoginEmailBox.Text,
                Password = LoginPassBox.Password
            };
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await client.PostAsJsonAsync(App_Path + "/Auth/Login", creditals);
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(ex.Message, ex.StatusCode.ToString());
                    return;
                }

                var deserializedResponse = response.Content.ReadFromJsonAsync<UserManagerResponse>().Result;

                if (!deserializedResponse.IsSuccess)
                {
                    MessageBox.Show(deserializedResponse.Message);
                    return;
                }

                using (var db = new UserContext(string.Empty))
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

                    AccessToken = token.Value;

                    db.SaveChanges();

                    DialogResult = true;
                    Close();
                }
            }
        }
    }
}
