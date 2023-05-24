namespace Phone;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
		
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		string code = await DisplayPromptAsync("PCの追加", "6桁のコードを入力してください。", maxLength : 6, keyboard : Keyboard.Numeric);
    }

    private void ContentPage_Loaded_1(object sender, EventArgs e)
    {
        DeviceName.Text = "このデバイスの名前(Model)\n" + DeviceInfo.Current.Model;
        UserName.Text = "Manufacturer\n" + DeviceInfo.Current.Manufacturer;
    }
}