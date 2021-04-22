using System;
using System.Linq;

namespace QS.Utilities.Text
{
    public static class StringValidationHelper
    {
        public static bool ContainsOnlyASCIICharacters(string str)
        {
            if(str is null) {
                throw new ArgumentNullException(nameof(str));
            }
            return str.All(ch => ch < 128);
        }
    }
}
