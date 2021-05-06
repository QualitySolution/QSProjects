using System.Security;

namespace QS.Utilities.Text
{
    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string str)
        {
            SecureString secureString = new SecureString();
            foreach (var item in str)
            {
                secureString.AppendChar(item);
            }
            return secureString;
        }
    }
}
