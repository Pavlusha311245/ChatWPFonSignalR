using Client.Data;
using System;
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

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ChatViewModel();
            DataContext = viewModel;

            using (db = new UserContext())
            {
                var token = db.Tokens.FirstOrDefault();
                if (token == null || token.ExpireDate < DateTime.Now)
                {
                    var auth = new AuthWindow();
                    if (auth.ShowDialog() == false)
                        Close();
                }                
            }            
        }

        protected override async void OnActivated(EventArgs e)
        {
            await viewModel.Connect();
        }

        protected override async void OnDeactivated(EventArgs e)
        {
            await viewModel.Disconnect();
        }
    }
}
