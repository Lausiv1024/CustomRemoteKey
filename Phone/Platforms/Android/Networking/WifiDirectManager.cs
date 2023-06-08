using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Net.Wifi.P2p;
using Android.Runtime;
using AndroidX.Fragment.App;
using Java.Net;

namespace Phone.Networking
{
    internal class WifiDirectManager
    {
        private WifiP2pManager _p2pManager;
        private bool _retryChannel;

        private readonly IntentFilter filter = new IntentFilter();

        private WifiP2pManager.Channel channel;
        private BroadcastReceiver receiver;

        public bool IsWifiDirectEnabled { get; set; }


        internal class ActionListener : Java.Lang.Object, WifiP2pManager.IActionListener
        {
            private readonly Context context;
            private readonly string failure;
            private readonly Action action;

            public ActionListener(Context context, string failure, Action action)
            {
                this.context = context;
                this.failure = failure;
                this.action = action;
            }
            public void OnFailure([GeneratedEnum] WifiP2pFailureReason reason)
            {
            }

            public void OnSuccess()
            {
                
            }
        }

        public void OnChannelDisconnected()
        {
            
        }

        public void ShowDetails(WifiP2pDevice device)
        {
            //var fragment = FragmentManager.FindFragment
            
        }

        public void Connect(WifiP2pConfig config)
        {
            _p2pManager.Connect(channel, config, new ActionListener(this, "Connect", () =>
            {

            }));
        }
    }
}
