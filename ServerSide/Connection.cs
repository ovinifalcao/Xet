﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SharedCommunication;
using Newtonsoft.Json;

namespace ServerSide
{
    public class Connection
    {
        TcpClient tcpClient;
        private Thread thrSender;
        private StreamReader streamReciver;
        private StreamWriter streamSender;
        private string Resquest;

        public Connection(TcpClient tcpCon)
        {
            this.tcpClient = tcpCon;
            this.thrSender = new Thread(AcceptClient);
            thrSender.Start();
        }

        private void AcceptClient()
        {
            streamReciver = new StreamReader(tcpClient.GetStream());
            streamSender = new StreamWriter(tcpClient.GetStream());

            var UserMessage = streamReciver.ReadLine();
            var objRecivedMessage = JsonConvert.DeserializeObject<ComnModel>(UserMessage);
            var objRecivedContent = JsonConvert.DeserializeObject<ContentSendConnectionResquest>(objRecivedMessage.Content);

            var objMessageToSend = new ComnModel()
            {
                Moment = DateTime.Now,
                ContentAction = ComnModel.Actions.SendConnectionSuccessful,
                Content = JsonConvert.SerializeObject(
                    new ContentSendConnectionSuccessful()
                    {
                        AceptedUserName = objRecivedContent.UserName,
                        UserPhoto = objRecivedContent.UserPhoto
                    })
            };

            if (!string.IsNullOrEmpty(UserMessage) &&
                ChatServer.DicOfConnections.ContainsValue(UserMessage) == false)
            {
                ChatServer.AddUser(tcpClient, objRecivedContent.UserName);
                ChatServer.WriteMessageOnStream(
                    tcpClient, JsonConvert.SerializeObject(objMessageToSend));
                WaitForMessege();
            }
            else
            {

                objMessageToSend.ContentAction = ComnModel.Actions.SendError;
                objMessageToSend.Content = JsonConvert.SerializeObject( new ContentSendError() { errorCod = 601 });
              
                ChatServer.WriteMessageOnStream(
                    tcpClient, JsonConvert.SerializeObject(objMessageToSend));

                CloseConnection();
            }
        }

        private void CloseConnection()
        {
            tcpClient.Close();
            streamReciver.Close();
            streamSender.Close();
        }

        private void WaitForMessege()
        {
            try
            {
                Resquest = streamReciver.ReadLine();
                while (!string.IsNullOrEmpty(Resquest))
                {
                    ChatServer.SenderActionRouter(Resquest);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error - Waiting for messages: " + ex.ToString());
                //remover usuário???
            }
        }

    }
}
