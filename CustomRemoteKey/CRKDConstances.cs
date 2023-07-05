using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRemoteKey
{
    internal class CRKDConstances
    {
        internal readonly Guid ConnectionHelperService = new Guid("f185cf48-84fc-46c4-b97f-67e2d79e43f0");

        internal readonly Guid ConnectionHelperIPAddr_Write = new Guid("7691ee00-acaa-461e-aadf-1f75681c4876");

        internal readonly Guid ConnectionHelperSecurityKey_Read = new Guid("113498a3-f4a1-448d-bee6-b2b017ed6932");

        internal const int AES_BLOCK_SIZE = 128;
        internal const int AES_KEY_SIZE = 256;
    }
}
