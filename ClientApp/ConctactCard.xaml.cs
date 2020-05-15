using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp
{
    public partial class ConctactCard : UserControl
    {
        public Byte[] UserProfilePhoto { get; set; }

        public enum ConversationSide
        {
            host = 0,
            Contact = 1
        }

        public List<Tuple<string, DateTime, ConversationSide>> Conversation { get; set; }

        public ConctactCard()
        {
            InitializeComponent();
            Conversation = new List<Tuple<string, DateTime, ConversationSide>>();
        }

        internal void UpdateBrief(string briefMsg)
        {
            var msgContenCount = briefMsg.Count();
            if (msgContenCount > 30) msgContenCount = 30;
            txbBriefMsg.Text = briefMsg.Substring(0, msgContenCount) + "...";
        }

        internal void UpdateContactPhoto(Byte[] ImgArr)
        {
            if (ImgArr != null)
            {
                UserProfilePhoto = ImgArr;
                Ellipse elProfileHost = new Ellipse
                {
                    Fill = new ImageBrush
                    {
                        ImageSource = FileOperations.RecoverImgFromArr(UserProfilePhoto)
                    }
                };
                pnProfilePhoto.Children.Clear();
                pnProfilePhoto.Children.Add(elProfileHost);
            }
        }
    }
}
