using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using SharedCommunication;
using Newtonsoft.Json;

namespace ClientApp
{
    public delegate void ActionBetweenThreads(ComnModel response);

    public class ConnectionController
    {

        private StreamWriter streamSender;
        private StreamReader streamReciver;
        private TcpClient tcpServer;
        private Thread messageThread;
        private MainWindow WdMensseger  = new MainWindow();
        private string UserName;
        private bool isConnected = false;


        public void InitializeConnection(string userName, string IpAdress, Byte[] UserPhoto = null)
        {
            try
            {
                tcpServer = new TcpClient();
                tcpServer.Connect(IPAddress.Parse(IpAdress), 4040);
                isConnected = true;
                this.UserName = userName;

                SendMessege(new ComnModel()
                {
                    ContentAction = ComnModel.Actions.SendConnectionResquest,
                    Moment = DateTime.Now,
                    Content = JsonConvert.SerializeObject(
                        new ContentSendConnectionResquest()
                        {
                            UserName = userName,
                            UserPhoto = UserPhoto
                        })
                });

                messageThread = new Thread(new ThreadStart(ReciveMensagens));
                messageThread.Start();
                WdMensseger.Closed += WdMensseger_Closed;
                WdMensseger.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro : " + ex.Message, "Erro ao conectar com servidor", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WdMensseger_Closed(object sender, EventArgs e)
        {
            SendMessege(
                new ComnModel()
                {
                    Addresee = null,
                    ContentAction = ComnModel.Actions.SendUserIsDisconnecting,
                    Moment = DateTime.Now,
                    Content = JsonConvert.SerializeObject(
                        new ContentSendUserIsDisconnecting()
                        {
                            Client = UserName
                        })
                });

            isConnected = false;
            streamSender.Close();
            streamReciver.Close();
            tcpServer.Close();
            App.Current.Shutdown();
        }

        public void SendMessege(ComnModel Message)
        {
            streamSender = new StreamWriter(tcpServer.GetStream());
            streamSender.WriteLine(JsonConvert.SerializeObject(Message));
            streamSender.Flush();

        }

        private void ReciveMensagens()
        {
            streamReciver = new StreamReader(tcpServer.GetStream());
            MessageRouter(streamReciver.ReadLine());

            while (isConnected)
            {
                MessageRouter(streamReciver.ReadLine());
            }
        }

        private void MessageRouter(string message)
        {
            var objComm = JsonConvert.DeserializeObject<ComnModel>(message);

            switch (objComm.ContentAction)
            {
                case ComnModel.Actions.SendText:
                    //atualizar o form Pai com uma nova mensagem

                    break;

                case ComnModel.Actions.SendImage:
                    //Atulizar o form pai com uma mensagem contento uma imagem
                    break;

                case ComnModel.Actions.SendTextGroup:
                    //atulizar o form pai com nova mensagem de grupo
                    break;

                case ComnModel.Actions.SendImageGroup:
                    //atulizar o fom pai com uma mensagem contendo uma imagem para um grupo
                    break;

                case ComnModel.Actions.SetGroup:
                    //configura um grupo no contexto;
                    break;

                case ComnModel.Actions.SendConnectionSuccessful:
                    WdMensseger.Dispatcher.Invoke(new ActionBetweenThreads(WdMensseger.ChangeClientDisplayedName), (new List<object>() { objComm }).ToArray());
                    break;

                case ComnModel.Actions.SendNewUSerConneted:
                    WdMensseger.Dispatcher.Invoke(new ActionBetweenThreads(WdMensseger.AddContactCardToThePanel), (new List<object>() { objComm }).ToArray());
                    break;

                case ComnModel.Actions.SendUsersAlreadyLogged:
                    WdMensseger.Dispatcher.Invoke(new ActionBetweenThreads(WdMensseger.AddAlreadyLoggedContactCardToThePanel), (new List<object>() { objComm }).ToArray());
                    break;

            }
        }


    }
}
