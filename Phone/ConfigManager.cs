using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone
{
    internal class ConfigManager
    {
        public static async Task SaveAccessKey(string IpAddr, string accessKey)
        {
            await SecureStorage.Default.SetAsync("IP-" + IpAddr, accessKey);
        }

        public static async Task<string> GetAccessKeyFromIP(string ipAddr)
        {
            return await SecureStorage.Default.GetAsync("IP-" + ipAddr);
        }
    }
}
