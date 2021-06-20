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
        string accessToken = string.Empty;
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public MainWindow()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            InitializeComponent();

            //using (db = new UserContext(string.Empty))
            //{
            //    user = db.Users.FirstOrDefault();

            //    if (user == null)
            //        this.accessToken = AuthorizationForm();

            //    accessToken = db.Tokens.FirstOrDefault(token => token.UserId == user.Id).ExpireDate < DateTime.Now ? null : db.Tokens.FirstOrDefault(token => token.UserId == user.Id).Value;

            //    if (accessToken == null)
            //        this.accessToken = AuthorizationForm();

            //    viewModel = new ChatViewModel(db.Tokens.FirstOrDefault(token => token.UserId == user.Id).Value);
            //}

            viewModel = new("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ6YXZhZHNraXkucGF2ZWwyMDAyKzNAb3V0bG9vay5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiemF2YWRza2l5LnBhdmVsMjAwMiszQG91dGxvb2suY29tIiwianRpIjoiZWM3ZmVkZmMtMDEyNS00MDA1LWJjY2ItNGVmYjA2OTQwYmY2IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsIm5iZiI6MTYyMzk5NTI3MSwiZXhwIjoxNjI0NjAwMDcxLCJpc3MiOiJodHRwczovL2dna3R0ZC5ieSIsImF1ZCI6Imh0dHBzOi8vZ2drdHRkLmJ5In0.-JcwOLXxcbP9YD0ao2NwFXqTThfoWJ_qOfDCmpdsjK8");

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
            }
        }

        private void OpenSettings(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Grid grid = new();
            grid.Width = 400;
            grid.Height = 200;
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            grid.RowDefinitions[0].Height = new GridLength(40, GridUnitType.Pixel);
            grid.RowDefinitions[1].Height = GridLength.Auto;
            grid.ColumnDefinitions[0].Width = new GridLength(360, GridUnitType.Pixel);
            grid.ColumnDefinitions[1].Width = new GridLength(40, GridUnitType.Pixel);

            PackIcon closeIcon = new();
            closeIcon.Kind = PackIconKind.Close;

            Button closeBtn = new()
            {
                Width = 30,
                Height = 30,
                Padding = new Thickness(0, 0, 0, 0),
                Content = closeIcon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            TextBlock title = new()
            {
                Text = "Настройки",
                VerticalAlignment =
                VerticalAlignment.Center,
                Padding = new Thickness(10, 0, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
            };

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

            closeBtn.Click += new RoutedEventHandler(CloseModal);

            grid.Children.Add(closeBtn);
            grid.Children.Add(buttonsGrid);
            grid.Children.Add(title);

            closeBtn.SetValue(Grid.ColumnProperty, 1);
            closeBtn.SetValue(Grid.ColumnProperty, 1);
            title.SetValue(Grid.ColumnProperty, 0);
            title.SetValue(Grid.RowProperty, 0);
            buttonsGrid.SetValue(Grid.ColumnProperty, 0);
            buttonsGrid.SetValue(Grid.RowProperty, 1);

            //buttonsGrid.Children.Add(toggleButton);
            //toggleButton.SetValue(Grid.ColumnProperty, 0);
            //toggleButton.SetValue(Grid.RowProperty, 0);

            DialogHost.Show(grid, "settings");
        }

        private void CloseModal(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("settings");
        }

        private void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
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
    }
}
