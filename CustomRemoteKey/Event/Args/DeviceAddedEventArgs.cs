using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRemoteKey.Event.Args
{
    public class DeviceAddedEventArgs
    {
        public DeviceAddedEventArgs(string deviceName, Guid deviceId, bool isNewDevice)
        {
            DeviceName = deviceName;
            DeviceId = deviceId;
            IsNewDevice = isNewDevice;
        }
        public string DeviceName { get; set; }
        public Guid DeviceId { get; set; }
        public bool IsNewDevice { get; set; }
    }
}
