using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CustomRemoteKey.Behaviours
{
    public abstract class BehaviourBase
    {
        public string Name { get; protected set; }
        public string DisplayName { get; set; }

        public BehaviourBase(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public abstract bool OnButtonPressed();

        public abstract bool OnButtonReleased();

        public abstract void DeploySettingUI(SimpleStackPanel parent);
    }
}
