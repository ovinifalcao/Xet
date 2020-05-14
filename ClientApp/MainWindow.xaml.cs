using SharedCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;



namespace ClientApp
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ChangeClientDisplayedName(ComnModel response)
        {
            txbUserName.Text = JsonConvert.DeserializeObject<ContentSendConnectionSuccessful>(response.Content).AceptedUserName;
        }

        public void AddContactCardToThePanel(ComnModel response)
        {
            var NewContactInfo = JsonConvert.DeserializeObject<ContentSendNewUserConnected>(response.Content);
            var Card = new ConctactCard();
            Card.txbContactName.Text = NewContactInfo.UserAddedName;
            pnContactCard.Children.Add(Card);
        }

        public void AddAlreadyLoggedContactCardToThePanel(ComnModel response)
        {
            var NewContactInfo = JsonConvert.DeserializeObject<ContentSendUsersAlreadyLogged>(response.Content);
            foreach (string st in NewContactInfo.AlreadyLoggedUsers)
            {
                var Card = new ConctactCard();
                Card.txbContactName.Text = st;
                pnContactCard.Children.Add(Card);
            }
        }


        public void RemoveCardOfDisconnectedContact(ComnModel response)
        {
            var ContactInfo = JsonConvert.DeserializeObject<ContentSendUserIsDisconnecting>(response.Content);
            var CardToRemove = pnContactCard.Children.Cast<ConctactCard>().
                    FirstOrDefault(N => N.txbContactName.Text == ContactInfo.Client);

            if (CardToRemove != null)
            {
                pnContactCard.Children.Remove(CardToRemove);
            }           
        }
    }
}
