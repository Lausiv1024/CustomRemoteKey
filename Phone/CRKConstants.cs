using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Phone
{
    public class CRKConstants
    {
        internal const int AES_BLOCKSIZE = 128;
        internal const int AES_KEYSIZE = 256;
        internal const int AES_IVSIZE = 128;

        internal const int BUFFER_SIZE = 1024;

        internal const int TCP_PORT = 60001;
    }
}
