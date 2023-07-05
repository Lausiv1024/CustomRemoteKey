using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomRemoteKey
{
    public class Util
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr obj);

        public static ImageSource ToImageSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle,
                    IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            } finally
            {
                DeleteObject(handle);
            }
        }

        public static BitmapImage ToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static string[] GetIpAddr()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] list = Dns.GetHostAddresses(hostName);
            var ipv4 = list.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToArray();
            string[] strIp = new string[ipv4.Length];
            for (int i = 0; i < strIp.Length; i++) strIp[i] = ipv4[i].ToString();
            return strIp;
        }

        public static byte[] Hash(byte[] data)
        {
            byte[] hashData;
            using (var sha256 = SHA256.Create())
            {
                hashData = sha256.ComputeHash(data);
            }
            return hashData;
        }

        public static void CreateRSAKeys(out string publicKey, out string privatekey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ToXmlString(false);
            privatekey = rsa.ToXmlString(true);
            
        }
    }
}
