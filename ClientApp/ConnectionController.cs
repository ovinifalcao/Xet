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

namespace ClientSide
{
    class ConnectionController
    {
        private StreamWriter streamSender;
        private StreamReader streamReciver;
        private TcpClient tcpServer;
        private Thread messageThread;

        private void InitializeConnection(string UserName, string IpAdress, Byte[] UserPhoto = null)
        {
            try
            {
                tcpServer = new TcpClient();
                tcpServer.Connect(IPAddress.Parse(IpAdress), 4040);

                SendMessege(new ComnModel()
                {
                    ContentAction = ComnModel.Actions.SendConnectionResquest,
                    Moment = DateTime.Now,
                    Content = new ContentSendConnectionResquest()
                    {
                        UserName = UserName,
                        UserPhoto = UserPhoto
                    }
                });
                messageThread = new Thread(new ThreadStart(RecebeMensagens));
                messageThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro : " + ex.Message, "Erro ao conectar com servidor", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SendMessege(ComnModel Message)
        {
            streamSender = new StreamWriter(tcpServer.GetStream());
            streamSender.WriteLine(JsonConvert.SerializeObject(Message));
            streamSender.Flush();
        }


        private void RecebeMensagens()
        {
            streamReciver = new StreamReader(tcpServer.GetStream());
            string ConResposta = streamReciver.ReadLine();

        }

    }
}
