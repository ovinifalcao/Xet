using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientApp
{
    public partial class MainConnection : Window
    {
        public string ProfilePhotoSource { get; private set; }

        public MainConnection()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var Controller = new ConnectionController();
            Controller.InitializeConnection(txbUserName.Text, txbIp.Text, FileOperations.BuildImgArr(ProfilePhotoSource));
            this.Hide();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Image Files (*.jpg)|*.jpg| (*.png*)|*.png*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                ProfilePhotoSource = openFileDialog.FileName;
                BitmapImage btmp = new BitmapImage(new Uri(ProfilePhotoSource, UriKind.Relative));

                vbUserPhoto.Child = null;

                var El = new Ellipse();
                El.Width = 80;
                El.Height = 80;

                ImageBrush imgBrush = new ImageBrush();
                imgBrush.ImageSource = btmp;
                El.Fill = imgBrush;

                vbUserPhoto.Child = El;
            }          
        }
    }
}
