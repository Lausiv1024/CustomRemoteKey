using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRemoteKey.Behaviours
{
    internal class Registry
    {
        public static readonly Dictionary<string, Type> BEHAVIOURS = new Dictionary<string, Type>();

        public static void RegisterBehaviours()
        {
            BEHAVIOURS.Add("ホットキー", typeof(BehaviourBase));
            BEHAVIOURS.Add("音声の再生", typeof(PlaySound));
        }
    }
}
