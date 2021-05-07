using System;
using System.Runtime.InteropServices;
using System.Security;

namespace QS.Utilities.Text
{
    public static class SecureStringExtensions
    {
        public static string ToPlainString(this SecureString secureString)
        {
            IntPtr strptr = Marshal.SecureStringToBSTR(secureString);
            string str = Marshal.PtrToStringBSTR(strptr);
            Marshal.ZeroFreeBSTR(strptr);
            return str;
        }
    }
}
