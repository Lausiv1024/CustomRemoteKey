using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommunityToolkit.Maui.Alerts;

namespace Phone;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{

		InitializeComponent();
		
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        //string code = await DisplayPromptAsync("PCの追加", "6桁のコードを入力してください。", maxLength : 6, keyboard : Keyboard.Numeric)
        PermissionStatus isCameraAllowed = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (isCameraAllowed != PermissionStatus.Granted)
        {
            if (isCameraAllowed == PermissionStatus.Disabled || isCameraAllowed == PermissionStatus.Unknown)
            {
                var a = Toast.Make("カメラはこのデバイスでサポートされていません．");
                await a.Show();
                return;
            }
            await DisplayAlert("PCの追加", "QRの読み取りでカメラを使用します．\nこの後のダイアログでカメラの使用を許可してください．", "OK");
            PermissionStatus status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                var a = Toast.Make("カメラを使用するには許可が必要です．");
                await a.Show();
                return;
            }
        }
        await Navigation.PushAsync(new QRReading());
    }

    private void ContentPage_Loaded_1(object sender, EventArgs e)
    {
        DeviceName.Text = "このデバイスの名前(Model)\n" + DeviceInfo.Current.Model;
        UserName.Text = "Manufacturer\n" + DeviceInfo.Current.Manufacturer;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
    }

    private async void Host2IPBtn_Clicked(object sender, EventArgs e)
    {
        string HostName = await DisplayPromptAsync("Host2IP", "ホスト名(PC名)の入力");
        if (HostName == null) return;
        try
        {
            IPHostEntry ip = Dns.GetHostEntry(HostName, AddressFamily.InterNetwork);
            await DisplayAlert("Success", ip.AddressList.ToArray()[0].ToString(), "OK");
        } catch(Exception ex)
        {
            await DisplayAlert("えらー", $"ホストのIPが見つかりませんでした\n例外メッセージ：{ex.Message}", "OK");
        }
        
    }

    private async void IP2Host_Clicked(object sender, EventArgs e)
    {
        string ipAddr = await DisplayPromptAsync("IP2Host", "IPアドレスの入力");
        if (ipAddr == null) return;
        //try
        //{
        //    IPHostEntry ip = Dns.GetHostEntry(ipAddr);
        //    await DisplayAlert("Success", ip.AddressList.ToArray()[0].ToString(), "OK");
        //} catch (Exception ex)
        //{
        //    await DisplayAlert("えらー", $"ホストのIPが見つかりませんでした\n例外メッセージ：{ex.Message}", "OK");
        //}
        try
        {
            var hosts = Dns.GetHostEntry("MainLsv");
            
            string hostList = "ホスト名\n";
            foreach (var host in hosts.AddressList)
            {
                Console.WriteLine(host.ToString());
                hostList += host.ToString() + "\n";
            }
        } catch (SocketException ex)
        {
            Console.WriteLine("Can't find MainLsv");
            await DisplayAlert("えらー", $"ホストのIPが見つかりませんでした\n例外メッセージ：{ex.Message}", "OK");
        }
    }

}