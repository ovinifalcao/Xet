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
        public ConctactCard OpenedConversation { get; private set; }
        public ConnectionController Controller { get; set; }

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
            Card.PreviewMouseDown += ContactCard_Click;
            Card.txbContactName.Text = NewContactInfo.UserAddedName;
            pnContactCard.Children.Add(Card);

        }

        public void AddAlreadyLoggedContactCardToThePanel(ComnModel response)
        {
            var NewContactInfo = JsonConvert.DeserializeObject<ContentSendUsersAlreadyLogged>(response.Content);
            foreach (string st in NewContactInfo.AlreadyLoggedUsers)
            {
                var Card = new ConctactCard();
                Card.PreviewMouseDown += ContactCard_Click;
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

        public void UpdateReceivedMessages(ComnModel response)
        {
            var TextContent = JsonConvert.DeserializeObject<ContentSendText>(response.Content);

            var CardOfSenderUser = pnContactCard.Children.Cast<ConctactCard>()
                .FirstOrDefault(c => c.txbContactName.Text == TextContent.SenderUserName);

            CardOfSenderUser.Conversation.Add(
                new Tuple<string, DateTime, ConctactCard.ConversationSide>
                (
                    TextContent.MessageContent,
                    response.Moment,
                     ConctactCard.ConversationSide.Contact
                ));
            CardOfSenderUser.UpdateBrief(TextContent.MessageContent);
    
            CardOfSenderUser.elNewMsgWarning.Visibility = Visibility.Visible;

            if (OpenedConversation != null && CardOfSenderUser == OpenedConversation)
            {
                AddNewMsgsToPanel(
                    new List<Tuple<string, DateTime, ConctactCard.ConversationSide>>
                    {
                    OpenedConversation.Conversation.Last()
                    });
            }
        }





        private void AddNewMsgsToPanel(List<Tuple<string, DateTime, ConctactCard.ConversationSide>> MsgdsToAdd  )
        {
            foreach (Tuple<string, DateTime, ConctactCard.ConversationSide> LastMsg in MsgdsToAdd)
            {
                var PlotBallon = new ControlMessage();
                PlotBallon.txbMessageContent.Text = LastMsg.Item1;
                PlotBallon.Margin = new Thickness(5);
                PlotBallon.HorizontalAlignment = HorizontalAlignment.Right;

                if (LastMsg.Item3 == ConctactCard.ConversationSide.Contact)
                    PlotBallon.HorizontalAlignment = HorizontalAlignment.Left;

                pnMessagePlot.Children.Add(PlotBallon);
            }
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            PerformSendMessage();
        }

        private void PerformSendMessage()
        {
            var MessageContent = StringFromRichTextBox(txbTypeMessege).TrimEnd(new char[] { '\n', '\r' });
            var Instant = DateTime.Now;

            OpenedConversation.Conversation.Add(
                new Tuple<string, DateTime, ConctactCard.ConversationSide>(
                     MessageContent,
                     Instant,
                     ConctactCard.ConversationSide.host));

            Controller.SendMessege(new ComnModel()
            {
                Addresee = OpenedConversation.txbContactName.Text,
                ContentAction = ComnModel.Actions.SendText,
                Moment = Instant,
                Content = JsonConvert.SerializeObject(
                    new ContentSendText()
                    {
                        MessageContent = MessageContent,
                        SenderUserName = txbUserName.Text
                    })
            });

            OpenedConversation.UpdateBrief(MessageContent);
            AddNewMsgsToPanel(
                 new List<Tuple<string, DateTime, ConctactCard.ConversationSide>>
                 {
                        OpenedConversation.Conversation.Last()
                 });
            txbTypeMessege.Document.Blocks.Clear();
        }

        string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd
            );
            return textRange.Text;
        }

        public void ContactCard_Click(object sender, RoutedEventArgs e)
        {
            pnMessagePlot.Children.Clear();
            OpenedConversation = (ConctactCard)sender;
            if (OpenedConversation.Conversation.Count > 0) { AddNewMsgsToPanel(OpenedConversation.Conversation); }
            if (pnWindTop.Visibility != Visibility.Visible) { pnWindTop.Visibility = Visibility.Visible; }
            if (pnWindBottom.Visibility != Visibility.Visible) { pnWindBottom.Visibility = Visibility.Visible; }

            OpenedConversation.elNewMsgWarning.Visibility = Visibility.Hidden;
            txbContactWindName.Text = OpenedConversation.txbContactName.Text;

        }

        private void TxbTypeMessege_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSendMessage();
            }
        }
    }
}
