using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

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
    }


}
