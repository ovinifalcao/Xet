using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using static ClientApp.ConctactCard;

namespace ClientApp
{
    public class FileOperations
    {
        public static Byte[] BuildImgArr(string fileSource)
        {
            if (string.IsNullOrEmpty(fileSource))
                return null;

            return File.ReadAllBytes(fileSource);
        }

        public static BitmapImage RecoverImgFromArr(Byte[] imgArr)
        {
            var image = new BitmapImage();
            using (var ms = new MemoryStream(imgArr))
            {
                ms.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = ms;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static void SaveConversation(List<Tuple<string, BitmapImage, DateTime, ConversationSide>> Conversation)
        {
            try
            {
                SaveFileDialog saveflDg = new SaveFileDialog();

                saveflDg.Filter = "txt files (*.txt)|*.txt";
                saveflDg.RestoreDirectory = true;

                if (saveflDg.ShowDialog() == true)
                {
                    using (var stWriter = new StreamWriter(saveflDg.FileName))
                    {
                        stWriter.Write(BuildTextConversation(Conversation));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Não foi possível salvar conversa", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string BuildTextConversation(List<Tuple<string, BitmapImage, DateTime, ConversationSide>> conversation)
        {
            string stContent = "";
            foreach (Tuple<string, BitmapImage, DateTime, ConversationSide> tp in conversation)
            {
                string textualContent = tp.Item1;
                string ConversationSide = "Internal";

                if (tp.Item2 != null) textualContent = "Imagem";
                if (tp.Item4 == ConctactCard.ConversationSide.Contact) ConversationSide = "External";

                stContent += string.Format("[{0} às {1}]: {2} \n \n", ConversationSide, tp.Item3.ToString(), textualContent);
            }
            return stContent;
        }
    }


}
