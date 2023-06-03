using Android.OS;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Phone;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{

		InitializeComponent();
		
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		string code = await DisplayPromptAsync("PC�̒ǉ�", "6���̃R�[�h����͂��Ă��������B", maxLength : 6, keyboard : Keyboard.Numeric);
        
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

    private void IPStatistics_Clicked(object sender, EventArgs e)
    {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPGlobalStatistics ipstat = properties.GetIPv4GlobalStatistics();
        Console.WriteLine("      Received ............................ : {0}",
     ipstat.ReceivedPackets);
        Console.WriteLine("      Forwarded ........................... : {0}",
        ipstat.ReceivedPacketsForwarded);
        Console.WriteLine("      Delivered ........................... : {0}",
        ipstat.ReceivedPacketsDelivered);
        Console.WriteLine("      Discarded ........................... : {0}",
        ipstat.ReceivedPacketsDiscarded);
    }
}