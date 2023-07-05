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
        //string code = await DisplayPromptAsync("PC�̒ǉ�", "6���̃R�[�h����͂��Ă��������B", maxLength : 6, keyboard : Keyboard.Numeric)
        PermissionStatus isCameraAllowed = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (isCameraAllowed != PermissionStatus.Granted)
        {
            if (isCameraAllowed == PermissionStatus.Disabled || isCameraAllowed == PermissionStatus.Unknown)
            {
                var a = Toast.Make("�J�����͂��̃f�o�C�X�ŃT�|�[�g����Ă��܂���D");
                await a.Show();
                return;
            }
            await DisplayAlert("PC�̒ǉ�", "QR�̓ǂݎ��ŃJ�������g�p���܂��D\n���̌�̃_�C�A���O�ŃJ�����̎g�p�������Ă��������D", "OK");
            PermissionStatus status = await Permissions.RequestAsync<Permissions.Camera>();

            if (status != PermissionStatus.Granted)
            {
                var a = Toast.Make("�J�������g�p����ɂ͋����K�v�ł��D");
                await a.Show();
                return;
            }
        }
        await Navigation.PushAsync(new QRReading());
    }

    private void ContentPage_Loaded_1(object sender, EventArgs e)
    {
        DeviceName.Text = "���̃f�o�C�X�̖��O(Model)\n" + DeviceInfo.Current.Model;
        UserName.Text = "Manufacturer\n" + DeviceInfo.Current.Manufacturer;
    }

    private void ContentPage_Disappearing(object sender, EventArgs e)
    {
    }

    private async void Host2IPBtn_Clicked(object sender, EventArgs e)
    {
        string HostName = await DisplayPromptAsync("Host2IP", "�z�X�g��(PC��)�̓���");
        if (HostName == null) return;
        try
        {
            IPHostEntry ip = Dns.GetHostEntry(HostName, AddressFamily.InterNetwork);
            await DisplayAlert("Success", ip.AddressList.ToArray()[0].ToString(), "OK");
        } catch(Exception ex)
        {
            await DisplayAlert("����[", $"�z�X�g��IP��������܂���ł���\n��O���b�Z�[�W�F{ex.Message}", "OK");
        }
        
    }

    private async void IP2Host_Clicked(object sender, EventArgs e)
    {
        string ipAddr = await DisplayPromptAsync("IP2Host", "IP�A�h���X�̓���");
        if (ipAddr == null) return;
        //try
        //{
        //    IPHostEntry ip = Dns.GetHostEntry(ipAddr);
        //    await DisplayAlert("Success", ip.AddressList.ToArray()[0].ToString(), "OK");
        //} catch (Exception ex)
        //{
        //    await DisplayAlert("����[", $"�z�X�g��IP��������܂���ł���\n��O���b�Z�[�W�F{ex.Message}", "OK");
        //}
        try
        {
            var hosts = Dns.GetHostEntry("MainLsv");
            
            string hostList = "�z�X�g��\n";
            foreach (var host in hosts.AddressList)
            {
                Console.WriteLine(host.ToString());
                hostList += host.ToString() + "\n";
            }
        } catch (SocketException ex)
        {
            Console.WriteLine("Can't find MainLsv");
            await DisplayAlert("����[", $"�z�X�g��IP��������܂���ł���\n��O���b�Z�[�W�F{ex.Message}", "OK");
        }
    }

}