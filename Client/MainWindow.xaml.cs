using Client.Data;
using Client.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            viewModel = new ChatViewModel();
            

            using (db = new UserContext())
            {
                if (db.Users.FirstOrDefault() == null || db.Tokens.Any(token => token.User == user && token.ExpireDate < DateTime.Now))
                {
                    var auth = new AuthWindow();
                    if (auth.ShowDialog() == false)
                        Close();
                }
                else
                {
                    user = db.Users.FirstOrDefault();
                    viewModel.UserName = $"{user.Name} {user.Surname}";
                }
            }

            DataContext = viewModel;
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

                var documentBtn = new Button()
                {
                    Content = Path.GetFileName(openFile.FileName),
                    Width = 150,
                    Height = 30
                };
                documentBtn.Click += DocumentBtn_Click;
                Documents.Children.Add(documentBtn);

                viewModel.File = result.ToArray();
                DataContext = viewModel;
            }
        }

        private void DocumentBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(sender.ToString());
        }
    }
}
