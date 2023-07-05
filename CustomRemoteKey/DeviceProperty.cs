using CustomRemoteKey.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRemoteKey
{
    public class DeviceProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public BehaviourBase[,] Behaviours;

        public string[,] ButtonName;

        public string PubKeyXML;

        public string PrivateKeyXML;

        public DeviceProperty()
        {
            Behaviours = new BehaviourBase[6, 20];
            ButtonName = new string[6, 20];
        }
    }
}
