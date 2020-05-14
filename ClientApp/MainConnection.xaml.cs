using System.Windows;

namespace ClientApp
{
    /// <summary>
    /// Lógica interna para MainConnection.xaml
    /// </summary>
    public partial class MainConnection : Window
    {
        public MainConnection()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var Controller = new ConnectionController();
            Controller.InitializeConnection(txbUserName.Text, txbIp.Text);
            this.Hide();
        }
    }
}
