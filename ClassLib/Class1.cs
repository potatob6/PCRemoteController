using System;
using System.Runtime.InteropServices;

namespace ClassLib
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct C2PMsg
    {
        public sbyte type;
        public byte[] strValue;
        public byte[] imgValue;
        public int len;
    }

}
