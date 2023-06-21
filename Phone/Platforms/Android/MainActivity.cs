using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Android.Net.Wifi.P2p;
using Android.Provider;
using Android.Util;
using Android.SE.Omapi;
using Android.Views;
using Android.Runtime;

namespace Phone
{
    [IntentFilter(new[] {Platform.Intent.ActionAppAction},
        Categories = new[] {Android.Content.Intent.CategoryDefault})]
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private BroadcastReceiver mReceiver;
        internal WifiP2pManager P2PManager;
        internal WifiP2pManager.Channel channel;
        public static MainActivity Instance;
        IntentFilter filter = new IntentFilter();

        public override void OnPostCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnPostCreate(savedInstanceState, persistentState);

            filter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
            filter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);

            mReceiver = new MyReceiver();
            
            P2PManager = (WifiP2pManager)GetSystemService(WifiP2pService);
            channel = P2PManager.Initialize(this, MainLooper, null);
            Instance = this;
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(mReceiver, filter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterReceiver(mReceiver);
        }

        public class MyReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                Toast.MakeText(context, "Receive broadcast: " + intent.Action,
                    ToastLength.Long).Show();
            }
        }

        private class MyActionListener : Java.Lang.Object, WifiP2pManager.IActionListener
        {
            private readonly Context _context;
            private readonly string _failure;
            private readonly Action _action;

            public MyActionListener(Context context, string failure, Action action)
            {
                _context = context;
                _failure = failure;
                _action = action;
            }

            public void OnFailure([GeneratedEnum] WifiP2pFailureReason reason)
            {
                
            }

            public void OnSuccess()
            {
                
            }
        }

        public void Connect(WifiP2pConfig config)
        {
            P2PManager.Connect(channel, config, new MyActionListener(this, "", () => { }));
        }

        public void Disconnect()
        {
            //var fragment = FragmentManager.FindFragmentById<DeviceDetailFragment>(Resource.Id.frag_detail);
        }

        //public async Task<string> ScanQRCode()
        //{
        //    MobileBarcodeScanner.Initialize(Application);
        //    var scanner = new MobileBarcodeScanner();
        //    var result = await scanner.Scan();
        //    return result.Text;
        //}
    }
}