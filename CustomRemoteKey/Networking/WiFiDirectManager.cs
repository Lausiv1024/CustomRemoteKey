using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;

namespace CustomRemoteKey.Networking
{
    internal class WiFiDirectManager
    {
        private static WiFiDirectManager instance;
        public static WiFiDirectManager Instance { get
            {
                return instance == null ? new WiFiDirectManager() : instance;
            } }
        
        private WiFiDirectManager() 
        {
            instance = this;
        }

        WiFiDirectAdvertisementPublisher AdvertisementPublisher { get; set; }

        //public ObservableCollection<>
    }
}
