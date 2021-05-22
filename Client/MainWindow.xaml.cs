using Client.Data;
using Client.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChatViewModel viewModel;
        UserContext db;
        User user;

        public MainWindow()
        {
            InitializeComponent();

            using (db = new UserContext())
            {
                user = db.Users.FirstOrDefault();

                if (user == null)
                    user = AuthorizationForm();

                Token accessToken = db.Tokens.FirstOrDefault(token => token.UserId == user.Id);

                if (accessToken == null && accessToken.ExpireDate < DateTime.Now)
                    user = AuthorizationForm();

                viewModel = new ChatViewModel(db.Tokens.FirstOrDefault(token => token.UserId == user.Id).Value);
            }

            DataContext = viewModel;
        }

        private User AuthorizationForm()
        {
            var auth = new AuthWindow();
            AuthWindow content = null;
            if (auth.ShowDialog() == true)
                content = auth.Content as AuthWindow;
            else Close();

            return content.userObject;
        }

        protected override async void OnActivated(EventArgs e)
        {
            if (!viewModel.IsConnected)
                await viewModel.Connect();
        }

        protected override async void OnClosed(EventArgs e)
        {
            await viewModel.Disconnect();
        }

        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new();
            openFile.Filter = "Doc files (*.doc;*.docx)|*.doc;*.docx|Text files (*.txt)|*.txt";
            openFile.Multiselect = false;

            if (openFile.ShowDialog() == true)
            {
                string extention = Path.GetExtension(openFile.FileName);
                var extentionBytes = new byte[5];
                System.Text.Encoding.UTF8.GetBytes(extention).CopyTo(extentionBytes, 0);
                var result = extentionBytes.Concat(File.ReadAllBytes(openFile.FileName).ToList());

                viewModel.File = result.ToArray();
                //DataContext = viewModel;
            }
        }
    }
}
