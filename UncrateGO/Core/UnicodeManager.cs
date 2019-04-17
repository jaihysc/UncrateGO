using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace UncrateGo.Core
{
    public static class UnicodeManager
    {
        public static bool ContainsUnicodeCharacter(string input)
        {
            const int maxAnsiCode = 255;

            return input.Any(c => c > maxAnsiCode);
        }

        //public static string EncodeToAsciiCharacters(string value)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (char c in value)
        //    {
        //        if (c > 127)
        //        {
        //            // This character is too big for ASCII
        //            string encodedValue = "\\u" + ((int)c).ToString("x4");
        //            sb.Append(encodedValue);
        //        }
        //        else
        //        {
        //            sb.Append(c);
        //        }
        //    }
        //    return sb.ToString();
        //}

        public static string DecodeToNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });



        }
    }
}
