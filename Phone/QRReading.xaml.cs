using Phone.Data;
using System.Security.Cryptography;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Phone;

public partial class QRReading : ContentPage
{
    bool scanned = false;
	public QRReading()
	{
		InitializeComponent();
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

    private async void Camera_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        if (scanned) return;
        scanned = true;
        string scannedText = args.Result[0].Text;
        await Camera.StopCameraAsync();
        Camera.BarCodeDetectionEnabled = false;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Processing.IsRunning = true;
        });
        //if (DebugMode.IsToggled)
        //{
        //    MainThread.BeginInvokeOnMainThread(async() =>
        //    {
        //        //await DisplayAlert("Detected Code", scannedText, "OK");
        //        await MakeToast(scannedText);
        //        await Task.Delay(1000);
        //        scanned = false;
        //        Camera.BarCodeDetectionEnabled = true;
        //        await Camera.StartCameraAsync();
        //    });
        //    if (scannedText.IndexOf("http://") == 0 || scannedText.IndexOf("https://") == 0)
        //    {
        //        Uri uri = new(scannedText);
        //        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        //    }

        //    return;
        //};

        
        var aes = Aes.Create();

        aes.BlockSize = CRKConstants.AES_BLOCKSIZE;
        aes.KeySize = CRKConstants.AES_KEYSIZE;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        aes.GenerateKey();
        byte[] keyData = new byte[aes.Key.Length + aes.IV.Length];
        Array.Copy(aes.Key, 0, keyData, 0, aes.Key.Length);
        Array.Copy(aes.IV, 0, keyData, aes.Key.Length, aes.IV.Length);
        DeviceAddingContext context = DeviceAddingContext.Parse(scannedText);
        await Task.Delay(500);
        if (context != null && context.IpAddr.Length == 0)
        {
            MainThread.BeginInvokeOnMainThread(async() =>
            {
                await MakeToast("Format Error");
            });
            return;
        }
        RSACryptoServiceProvider rsa = new();
        
        rsa.FromXmlString(context.RSAPublicKey);
        byte[] encryptedCommonKey = rsa.Encrypt(keyData, false);
        
        if (!await MainPage.Instance.ConnectTo(context.IpAddr, encryptedCommonKey, (byte[])aes.Key.Clone(), (byte[]) aes.IV.Clone()))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await MakeToast("Ú‘±Ž¸”s");
                await Navigation.PopAsync();
            });
            return;
        }
        aes.Clear();
        Camera.BarCodeDetectionEnabled = true;
        scanned = false;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await MakeToast("Success");
            await Navigation.PopAsync();
        });
    }

    private async Task MakeToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short, 14);
        CancellationTokenSource src = new();
        await toast.Show(src.Token);
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
        Camera.TorchEnabled = false;
    }

    private void Torch_Toggled(object sender, ToggledEventArgs e)
    {
        Camera.TorchEnabled = Torch.IsToggled;
    }
}