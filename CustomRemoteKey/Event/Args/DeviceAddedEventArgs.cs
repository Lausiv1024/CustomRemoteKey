using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRemoteKey.Event.Args
{
    public class DeviceAddedEventArgs
    {
        public DeviceAddedEventArgs(string deviceName, Guid deviceId)
        {
            DeviceName = deviceName;
            DeviceId = deviceId;
        }
        public string DeviceName { get; set; }
        public Guid DeviceId { get; set; }
    }
}
