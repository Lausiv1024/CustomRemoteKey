using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone.Data
{
    public class DeviceAddingContext
    {
        public string[] IpAddr;
        public string RSAPublicKey;
        public DeviceAddingContext() { }
        public static DeviceAddingContext Parse(string str)
        {
            
            if (str != null) 
            {
                var context = new DeviceAddingContext();
                var datas = str.Split(',');
                int i = 0;
                foreach (var data in datas)
                {
                    context.IpAddr = new string[datas.Length - 1];
                    if (data.IndexOf(GetKeyFromKeyName("AccessKey")) == 0)
                        context.RSAPublicKey = data.Replace(GetKeyFromKeyName("AccessKey"), "");
                    else
                        context.IpAddr[i] = data;
                    i++;
                }
                return context;
            }
            
            throw new FormatException("Invalid Format!!");
        }

        public static string GetKeyFromKeyName(string keyName) => keyName + "=";
    }
}
