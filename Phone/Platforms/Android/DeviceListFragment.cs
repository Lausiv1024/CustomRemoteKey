using Android.App;
using Android.Net.Wifi.P2p;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone.Platforms.Android
{
    internal class DeviceListFragment
    {
        private readonly List<WifiP2pDevice> _peers = new List<WifiP2pDevice>();
        private ProgressDialog _progressDialog;
        private View _contentView;
    }
}
