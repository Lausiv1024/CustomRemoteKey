namespace Phone
{
    public partial class MainPage : ContentPage
    {
        bool[,] hapticCooldown;
        private const int ProfileCount = 6;
        private const int ButtonCountX = 4;
        private const int ButtonCountY = 5;

        private int ProfileMode = 0;//そのうちデスクトップ側が全部管理するようになるんだ

        public MainPage()
        {
            InitializeComponent();

            AllParent.RowDefinitions[0].Height = DeviceDisplay.Current.MainDisplayInfo.Width /
                (ProfileCount * 3);

            for (int i = 0; i < ProfileCount; i++)
            {
                ProfileSelect.ColumnDefinitions.Add(new ColumnDefinition());
                Button but = new Button();
                but.Text = (i + 1).ToString();
                but.Margin = new Thickness(4);
                but.Pressed += OnProfileButtonPressed;
                Grid.SetColumn(but, i);
                ProfileSelect.Children.Add(but);
            }

            hapticCooldown = new bool[ButtonCountX,ButtonCountY];

            for (int i = 0; i < ButtonCountX; i++) ControlButtonDeck.AddColumnDefinition(new ColumnDefinition());
            for (int i = 0; i < ButtonCountY + 1; i++) ControlButtonDeck.AddRowDefinition(new RowDefinition());
            for (int i = 0; i < ButtonCountX * ButtonCountY; i++)
            {
                var button = new Button();

                button.Margin = new Thickness(6);
                button.Released += ControlButtonUp;
                button.Pressed += ControlButtonDown;
                Grid.SetColumn(button, i % ButtonCountX);
                Grid.SetRow(button, i / ButtonCountX);
                ControlButtonDeck.Children.Add(button);
            }

            Slider label = new Slider();
            label.Margin = new Thickness(10);
            //label.Background = new SolidColorBrush(Colors.Indigo);
            
            Grid.SetRow(label, ButtonCountY);
            Grid.SetColumnSpan(label, ButtonCountX);
            ControlButtonDeck.Children.Add(label);
        }

        private Button GetControlButton(int column, int row)
        {   
            if (row >= ButtonCountY || column >= ButtonCountX) return null;
            return ControlButtonDeck.Children.Cast<Button>()
                .Where(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == column).FirstOrDefault();
        }

        private void OnProfileButtonPressed(object sender, EventArgs e)
        {
            var button = sender as Button;
            ProfileMode = Grid.GetColumn(button);
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            GetControlButton(2,3).Text = ProfileMode.ToString();
        }

        private async void ControlButtonDown(object sender, EventArgs e)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            Button button = (Button)sender;
            if (ProfileMode == 1)
                await DisplayAlert("Pressed", $"Pressed Button : [{Grid.GetColumn(button)}, {Grid.GetRow(button)}]", "OK");
            hapticCooldown[Grid.GetColumn(button), Grid.GetRow(button)] = true;
            await Task.Delay(100);
            hapticCooldown[Grid.GetColumn(button),Grid.GetRow(button)] = false;
        }

        private void ControlButtonUp(object sender, EventArgs e)
        {
            Button button  = (Button)sender;
            if (!hapticCooldown[Grid.GetColumn(button), Grid.GetRow(button)])
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            DeviceDisplay.Current.KeepScreenOn = false;
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            DeviceDisplay.Current.KeepScreenOn = true;
        }
    }
}