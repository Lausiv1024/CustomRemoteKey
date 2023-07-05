using Phone.Data;
using System.Security.Cryptography;

namespace Phone;

public partial class QRReading : ContentPage
{
    bool scanned = false;
	public QRReading()
	{
		InitializeComponent();

        Camera.BarCodeOptions = new()
        {
            PossibleFormats = { ZXing.BarcodeFormat.QR_CODE }
        };
        Camera.BarCodeDetectionEnabled = true;
    }

    private void Camera_CamerasLoaded(object sender, EventArgs e)
    {
        if (Camera.Cameras.Count > 0)
        {
            Camera.Camera = Camera.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                await Camera.StopCameraAsync();
                await Camera.StartCameraAsync();
            });
        }
    }

    private void Camera_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        if (scanned) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Camera.BarCodeDetectionEnabled = false;
            scanned = true;
            await Task.Delay(500);
            var aes = Aes.Create();

            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();
            aes.GenerateKey();
            byte[] keyData = new byte[aes.Key.Length + aes.IV.Length];
            Array.Copy(aes.Key, 0, keyData, 0, aes.Key.Length);
            Array.Copy(aes.IV, 0, keyData, aes.Key.Length, aes.IV.Length);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            DeviceAddingContext context = DeviceAddingContext.Parse(args.Result[0].Text);

            rsa.FromXmlString(context.RSAPublicKey);
            byte[] encryptedCommonKey = rsa.Encrypt(keyData, false);

            Camera.BarCodeDetectionEnabled = true;

            //MainPage.Instance.ATTETEE = Camera.BarCodeDetectionEnabled;
            MainPage.Instance.ConnectTo(context.IpAddr, encryptedCommonKey);
            scanned = false;
            aes.Clear();
            await Navigation.PopAsync();
        });
    }
}