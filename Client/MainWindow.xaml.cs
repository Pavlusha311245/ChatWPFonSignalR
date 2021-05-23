using Client.Data;
using Client.Models;
using Microsoft.Win32;
using Server.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

            using (db = new UserContext(string.Empty))
            {
                user = db.Users.FirstOrDefault();

                if (user == null)
                    user = AuthorizationForm();

                Token accessToken = db.Tokens.FirstOrDefault(token => token.UserId == user.Id);

                if (accessToken == null && accessToken.ExpireDate < DateTime.Now)
                    user = AuthorizationForm();

                viewModel = new ChatViewModel(db.Tokens.FirstOrDefault(token => token.UserId == user.Id).Value);
            }

            viewModel.MessageTask = new Message();

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
            openFile.Multiselect = true;

            if (openFile.ShowDialog() == true)
            {
                Task task = new();

                foreach (var fileName in openFile.FileNames)
                {
                    var fileContent = File.ReadAllBytes(openFile.FileName);
                    var document = new Document
                    {
                        Extension = Path.GetExtension(fileName),
                        Content = fileContent,
                        Hash = Convert.ToBase64String(MD5.HashData(fileContent))
                    };
                    task.Documents.Add(document);
                }

                task.DeadLine = DateTime.Now;
                task.Remark = "";

                viewModel.MessageTask.Task = task;
                //DataContext = viewModel;
            }
        }
    }
}
