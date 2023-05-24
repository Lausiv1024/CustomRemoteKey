using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CustomRemoteKey.Behaviours
{
    public class PlaySound : BehaviourBase
    {
        public string AudioFilePath { get; set; }
        public PlaySound() : base("PlaySound", "音を鳴らす")
        {

        }

        public override bool OnButtonPressed()
        {
            if (File.Exists(AudioFilePath))
            {
                return true;
            }
            return false;
        }

        public override bool OnButtonReleased()
        {
            return true;
        }

        public override void DeploySettingUI(SimpleStackPanel parent)
        {
            
        }
    }
}
