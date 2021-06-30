using Client.Data;
using Client.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Notification.Wpf;
using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client
{
    public enum TabModals
    {
        Settings,
        Logout
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChatViewModel viewModel;
        UserContext db;
        User user;
        string accessToken = string.Empty;
        string openModal = string.Empty;
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            InitializeComponent();
            CheckAuth();
        }

        private void CheckAuth()
        {
            using (db = new UserContext(string.Empty))
            {
                user = db.Users.FirstOrDefault();

                if (user == null)
                    this.accessToken = AuthorizationForm();

                if (db.Tokens.FirstOrDefault() == null)
                    this.accessToken = AuthorizationForm();

                if (string.IsNullOrEmpty(accessToken))
                    accessToken = db.Tokens.FirstOrDefault().ExpireDate < DateTime.Now ? AuthorizationForm() : db.Tokens.FirstOrDefault().Value;

                viewModel = new ChatViewModel(accessToken);
            }

            DataContext = viewModel;
        }

        private string AuthorizationForm()
        {
            var auth = new AuthWindow();
            if (auth.ShowDialog() != true)
                Close();

            return auth.AccessToken;
        }

        protected override async void OnActivated(EventArgs e)
        {
            if (!viewModel.IsConnected)
                await viewModel.Connect();
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (viewModel != null)
                await viewModel.Disconnect();
        }

        private void ChooseFiles(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new();
            openFile.Filter = "Doc files (*.doc;*.docx)|*.doc;*.docx|Text files (*.txt)|*.txt";
            openFile.Multiselect = true;

            if (openFile.ShowDialog() == true)
            {
                Task task = new();
                List<Document> documents = new();

                foreach (var filepath in openFile.FileNames)
                {
                    var fileContent = File.ReadAllBytes(openFile.FileName);
                    documents.Add(new Document
                    {
                        FileName = Path.GetFileNameWithoutExtension(filepath),
                        Extension = Path.GetExtension(filepath),
                        Content = fileContent,
                        Hash = Convert.ToBase64String(MD5.HashData(fileContent))
                    });
                }

                var grid = GenerateGridModalWindows("Новая задача", 500, 150 + (openFile.FileNames.Count() * 30), 2, 4);

                grid.RowDefinitions[^2].Height = new GridLength(40);
                grid.RowDefinitions[^1].Height = new GridLength(40);

                Button sendTaskBtn = new();
                sendTaskBtn.HorizontalAlignment = HorizontalAlignment.Right;
                sendTaskBtn.Margin = new Thickness(0, 0, 20, 0);
                sendTaskBtn.Width = 100;
                sendTaskBtn.Height = 25;
                sendTaskBtn.Content = "Send task";
                sendTaskBtn.Command = viewModel.SendTaskCommand;

                TextBox remark = new();
                remark.HorizontalAlignment = HorizontalAlignment.Stretch;
                remark.VerticalAlignment = VerticalAlignment.Stretch;
                remark.Margin = new Thickness(20, 0, 20, 0);
                remark.Padding = new Thickness(0);

                sendTaskBtn.Click += (sender, e) =>
                {
                    NotificationManager notification = new();
                    notification.Show("Информация", "Задание успешно создано", NotificationType.Success);
                    SystemSounds.Exclamation.Play();
                    task.Remark = remark.Text;
                    DialogHost.CloseDialogCommand.Execute(null, null);
                };

                ListBox listDocs = new();
                listDocs.ItemsSource = documents;

                grid.Children.Add(listDocs);
                grid.Children.Add(sendTaskBtn);
                grid.Children.Add(remark);

                listDocs.SetValue(Grid.ColumnProperty, 0);
                listDocs.SetValue(Grid.ColumnSpanProperty, 2);
                listDocs.SetValue(Grid.RowProperty, 1);

                sendTaskBtn.SetValue(Grid.ColumnProperty, 0);
                sendTaskBtn.SetValue(Grid.ColumnSpanProperty, 2);
                sendTaskBtn.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 1);
                remark.SetValue(Grid.ColumnProperty, 0);
                remark.SetValue(Grid.ColumnSpanProperty, 2);
                remark.SetValue(Grid.RowProperty, grid.RowDefinitions.Count - 2);

                task.DeadLine = DateTime.Now;
                viewModel.SenderMessage.Task = task;
                viewModel.SenderMessage.Documents = documents;

                DialogHost.Show(grid);
            }
        }

        private Grid GenerateGridModalWindows(
            string title,
            int width,
            int height,
            int columnsCount = 1,
            int rowsCount = 1)
        {
            Grid grid = new();
            grid.Width = width;
            grid.Height = height;
            for (int i = 0; i < columnsCount; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < rowsCount; i++)
                grid.RowDefinitions.Add(new RowDefinition());

            PackIcon closeIcon = new();
            closeIcon.Kind = PackIconKind.Close;

            var titleBlock = new TextBlock()
            {
                Text = title,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(10, 0, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            Button closeBtn = new()
            {
                Width = 25,
                Height = 25,
                Padding = new Thickness(0, 0, 0, 0),
                Content = closeIcon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            closeBtn.MouseMove += new System.Windows.Input.MouseEventHandler((sender, e) =>
            {
                ((Button)sender).Width = 30;
                ((Button)sender).Height = 30;
            });

            closeBtn.MouseLeave += new System.Windows.Input.MouseEventHandler((sender, e) =>
            {
                ((Button)sender).Width = 25;
                ((Button)sender).Height = 25;
            });

            closeBtn.Click += (sender, e) => DialogHost.CloseDialogCommand.Execute(null, null);

            grid.RowDefinitions[0].Height = new GridLength(40, GridUnitType.Pixel);
            grid.ColumnDefinitions[grid.ColumnDefinitions.Count - 1].Width = new GridLength(40, GridUnitType.Pixel);

            grid.Children.Add(closeBtn);
            grid.Children.Add(titleBlock);

            closeBtn.SetValue(Grid.ColumnProperty, grid.ColumnDefinitions.Count - 1);
            closeBtn.SetValue(Grid.RowProperty, 0);
            titleBlock.SetValue(Grid.ColumnProperty, 0);
            titleBlock.SetValue(Grid.RowProperty, 0);

            return grid;
        }

        private TextBlock GenerateModalWindowTitle(string text)
        {
            TextBlock title = new()
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(10, 0, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            return title;
        }

        private void OpenSettings(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ;
            var grid = GenerateGridModalWindows("Настройки", 400, 200, 2, 2);

            Grid buttonsGrid = new();
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            buttonsGrid.RowDefinitions.Add(new RowDefinition());
            buttonsGrid.RowDefinitions.Add(new RowDefinition());

            //ToggleButton toggleButton = new();
            //toggleButton.IsChecked = paletteHelper.GetTheme().GetBaseTheme().Equals(new MaterialDesignDarkTheme());
            //toggleButton.Click += new RoutedEventHandler((object sender, RoutedEventArgs args) =>
            //{
            //    ITheme theme = paletteHelper.GetTheme();
            //    IBaseTheme baseTheme = (bool)((ToggleButton)sender).IsChecked ? new MaterialDesignDarkTheme() : new MaterialDesignLightTheme();
            //    theme.SetBaseTheme(baseTheme);
            //    paletteHelper.SetTheme(theme);
            //});


            grid.Children.Add(buttonsGrid);

            buttonsGrid.SetValue(Grid.ColumnProperty, 0);
            buttonsGrid.SetValue(Grid.RowProperty, 1);

            //buttonsGrid.Children.Add(toggleButton);
            //toggleButton.SetValue(Grid.ColumnProperty, 0);
            //toggleButton.SetValue(Grid.RowProperty, 0);

            DialogHost.Show(grid);
        }

        private void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            viewModel.SenderMessage.ChatID = ((Chat)ChatsList.SelectedItem).Id;

            if (SenderRow.Visibility == Visibility.Hidden)
            {
                SenderRow.Visibility = Visibility.Visible;
                MessagesView.Visibility = Visibility.Visible;
                UserChoisPrompt.Visibility = Visibility.Hidden;
            }
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text) && viewModel.SenderMessage.Task == null)
            {
                MessageBox.Show("Строка сообщений пуста");
            }
            MessageTextBox.Text = string.Empty;
        }

        private void Logout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = GenerateGridModalWindows("Выйти из аккаунта?", 300, 100, 2, 2);

            Button exitAccountBtn = new();
            exitAccountBtn.VerticalAlignment = VerticalAlignment.Center;
            exitAccountBtn.HorizontalAlignment = HorizontalAlignment.Center;
            exitAccountBtn.Width = 100;
            exitAccountBtn.Height = 30;
            exitAccountBtn.Content = "Выйти";
            exitAccountBtn.Background = Brushes.DarkRed;
            exitAccountBtn.MouseMove += new System.Windows.Input.MouseEventHandler((sender, e) =>
            {
                ((Button)sender).Background = Brushes.Red;
            });
            exitAccountBtn.MouseLeave += new System.Windows.Input.MouseEventHandler((sender, e) =>
            {
                ((Button)sender).Background = Brushes.DarkRed;
            });
            exitAccountBtn.Click += (sender, e) =>
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
                using (db = new UserContext(string.Empty))
                {
                    db.Tokens.Remove(db.Tokens.FirstOrDefault());
                    db.SaveChanges();
                    DataContext = null;
                    CheckAuth();
                }
            };

            grid.Children.Add(exitAccountBtn);

            exitAccountBtn.SetValue(Grid.ColumnProperty, 0);
            exitAccountBtn.SetValue(Grid.RowProperty, 1);
            exitAccountBtn.SetValue(Grid.ColumnSpanProperty, 2);

            DialogHost.Show(grid);
        }
    }
}
