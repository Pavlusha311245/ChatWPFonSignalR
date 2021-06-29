using Client.Data;
using Client.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Server.Models;
using System;
using System.IO;
using System.Linq;
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

            viewModel.MessageTask = new Message();
            viewModel.User = user;

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

                RoutedEventHandler evt = (sender, e) =>
                {
                    DialogHost.Close("newTask");
                    openModal = string.Empty;
                };
                var grid = GenerateGridModalWindows("Новая задача", evt, 500, 300, 2, 2);

                openModal = "newTask";
                DialogHost.Show(grid, "newTask");
            }
        }

        private Grid GenerateGridModalWindows(
            string title,
            RoutedEventHandler evt,
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

            closeBtn.Click += evt;

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
            RoutedEventHandler evt = (sender, e) =>
            {
                DialogHost.Close("settings");
                openModal = string.Empty;
            };
            var grid = GenerateGridModalWindows("Настройки", evt, 400, 200, 2, 2);

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

            if (!string.IsNullOrEmpty(openModal))
                CloseModal(openModal);

            openModal = "settings";
            DialogHost.Show(grid, "settings");
        }

        private void CloseModal(string identify) => DialogHost.Close(identify);

        private void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            viewModel.ReceivingChat = (Chat)ChatsList.SelectedItem;

            if (SenderRow.Visibility == Visibility.Hidden)
            {
                SenderRow.Visibility = Visibility.Visible;
                MessagesView.Visibility = Visibility.Visible;
                UserChoisPrompt.Visibility = Visibility.Hidden;
            }
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            MessageTextBox.Text = string.Empty;
        }

        private void Logout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RoutedEventHandler evt = (sender, e) =>
            {
                DialogHost.Close("exit");
                openModal = string.Empty;
            };
            var grid = GenerateGridModalWindows("Выйти из аккаунта?", evt, 300, 100, 2, 2);

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
                DialogHost.Close("exit");
                openModal = string.Empty;
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

            if (!string.IsNullOrEmpty(openModal))
                CloseModal(openModal);

            openModal = "exit";
            DialogHost.Show(grid, "exit");
        }
    }
}
