﻿using SharedCommunication;
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
            var AcceptedUserInfo = JsonConvert.DeserializeObject<ContentSendConnectionSuccessful>(response.Content);
            txbUserName.Text = AcceptedUserInfo.AceptedUserName;

            if (AcceptedUserInfo.UserPhoto != null)
            {
                elProfileHost.Fill = new ImageBrush
                {
                    ImageSource = FileOperations.RecoverImgFromArr(AcceptedUserInfo.UserPhoto)
                };
            }
            else
            {
                pnProfileFrame.Children.Remove(elProfileHost);
                
                var DftImg = new DefaultUserIcon();
                DftImg.HorizontalAlignment = HorizontalAlignment.Left;
                Margin = new Thickness(10, 0, 0, 0);

                pnProfileFrame.Children.Add(DftImg);
            }
        }

        public void AddContactCardToThePanel(ComnModel response)
        {
            var NewContactInfo = JsonConvert.DeserializeObject<ContentSendNewUserConnected>(response.Content);
            var Card = new ConctactCard();
            Card.PreviewMouseDown += ContactCard_Click;
            Card.txbContactName.Text = NewContactInfo.UserAddedName;
            Card.UpdateContactPhoto(NewContactInfo.UserAddedPhoto);
            pnContactCard.Children.Add(Card);

        }

        public void AddAlreadyLoggedContactCardToThePanel(ComnModel response)
        {
            var NewContactInfo = JsonConvert.DeserializeObject<ContentSendUsersAlreadyLogged>(response.Content);
            foreach (Tuple<string, Byte[]> st in NewContactInfo.AlreadyLoggedUsers)
            {
                var Card = new ConctactCard();
                Card.PreviewMouseDown += ContactCard_Click;
                Card.txbContactName.Text = st.Item1;
                Card.UpdateContactPhoto(st.Item2);
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

        public void RemoveContactCardToThePanel(ComnModel response)
        {
            var UserToRemove = JsonConvert.DeserializeObject<ContentSendUserIsDisconnecting>(response.Content);

            for(int i = 0; i < pnContactCard.Children.Count; i++)
            {
                if (((ConctactCard)pnContactCard.Children[i]).txbContactName.Text == UserToRemove.Client)
                {
                    if (((ConctactCard)pnContactCard.Children[i]) == OpenedConversation)
                        pnWindBottom.Visibility = Visibility.Hidden;

                    pnContactCard.Children.RemoveAt(i);
                    continue;
                }                    
            }
        }

        internal void AddNewGroupCardToThePanel(ComnModel response)
        {
            var NewGroupInfo = JsonConvert.DeserializeObject<ContentSetGroup>(response.Content);

            var Card = new ConctactCard();
            Card.PreviewMouseDown += ContactCard_Click;
            Card.txbContactName.Text = NewGroupInfo.GroupName;

            string ContactNames = "";
            foreach (string st in NewGroupInfo.ParticipantsNames)
            {
                ContactNames += st + ", "; 
            }
            Card.GroupContacts = ContactNames.TrimEnd(new char[] { ',', ' ' });
            pnContactCard.Children.Add(Card);

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
            try
            {
                var MessageContent = StringFromRichTextBox(txbTypeMessege).TrimEnd(new char[] { '\n', '\r' });
                if (string.IsNullOrEmpty(MessageContent)) return;

                var Instant = DateTime.Now;

                OpenedConversation.Conversation.Add(
                    new Tuple<string, DateTime, ConctactCard.ConversationSide>(
                         MessageContent,
                         Instant,
                         ConctactCard.ConversationSide.host));

                var msg = new ComnModel()
                {
                    Addresee = OpenedConversation.txbContactName.Text,
                    ContentAction = ComnModel.Actions.SendText,
                    Moment = Instant
                };

                if (!string.IsNullOrEmpty(OpenedConversation.GroupContacts))
                {
                    msg.Content = JsonConvert.SerializeObject(
                        new ContentSendTextGroup()
                        {
                            GroupName = OpenedConversation.txbContactName.Text,
                            Message = string.Format("[{0}]: {1}", txbUserName.Text, MessageContent),
                            Sender = txbUserName.Text
                        });

                    msg.ContentAction = ComnModel.Actions.SendTextGroup;
                }
                else
                {
                    msg.Content = JsonConvert.SerializeObject(
                    new ContentSendText()
                    {
                        MessageContent = MessageContent,
                        SenderUserName = txbUserName.Text
                    });
                }

                Controller.SendMessege(msg);

                OpenedConversation.UpdateBrief(MessageContent);
                AddNewMsgsToPanel(
                     new List<Tuple<string, DateTime, ConctactCard.ConversationSide>>
                     {
                        OpenedConversation.Conversation.Last()
                     });

                txbTypeMessege.Document.Blocks.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Erro ao enviar Mensagem", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            

            pnIdentifyConversation.Children.Clear();
            if (OpenedConversation.UserProfilePhoto == null)
            {
                pnIdentifyConversation.Children.Add(
                    new ConctactCard());
            }
            else
            {
                pnIdentifyConversation.Children.Add(new Ellipse
                {
                    Fill = new ImageBrush
                    {
                        ImageSource = FileOperations.RecoverImgFromArr(OpenedConversation.UserProfilePhoto)
                    }
                });
            }


            txbGroupInfo.Text = null;
            if (!string.IsNullOrEmpty(OpenedConversation.GroupContacts))
            {
                txbGroupInfo.Text = OpenedConversation.GroupContacts;
            }

        }

        private void TxbTypeMessege_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSendMessage();
            }
        }

        private void ButtonGroup_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var WdSelectUsers = new SelectUsers(pnContactCard.Children.Cast<ConctactCard>().ToList());
                WdSelectUsers.ShowDialog();

                if (string.IsNullOrEmpty(WdSelectUsers.txbGroupName.Text)) throw new Exception("Não é possível adicionar um gropo sem nome");

                var UserGroups = WdSelectUsers.GetAllSelected();
                UserGroups.Add(txbUserName.Text);

                if (UserGroups.Count < 3) throw new Exception("Não é possível iniciar um grupo de conversa com menos de dois contatos");
            
                Controller.SendMessege(new ComnModel()
                {
                    Addresee = null,
                    Moment = DateTime.Now,
                    ContentAction = ComnModel.Actions.SetGroup,
                    Content = JsonConvert.SerializeObject(
                        new ContentSetGroup()
                        {
                            GroupName = WdSelectUsers.txbGroupName.Text,
                            ParticipantsNames = UserGroups
                        })
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Erro em criar grupo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
