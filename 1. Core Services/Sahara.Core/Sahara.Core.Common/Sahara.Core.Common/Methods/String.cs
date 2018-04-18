using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class Strings
    {
        public static string ToCamelCase(this string the_string)
        {
            

            // If there are 0 or 1 characters, just return the string.
            if (the_string == null || the_string.Length < 2)
                return the_string;


            //Special case to ensure ID looks nice for json feeds:
            if (the_string.Substring(the_string.Length - 2) == "ID" || the_string.Substring(the_string.Length - 2) == "Id")
            {
                the_string = the_string.Remove(the_string.Length - 2, 2) + " Id";
            }

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }

        public static string ReplaceLeadingAndTrailingUnderscoresWithSpaces(string inStr)
        {
            var outStr = String.Empty;

            //Replace trailing Underscore
            if (inStr[(inStr.Length - 1)].ToString() == "_")
            {
                //Swap out leading "_" for " "
                char[] chars = inStr.ToCharArray();
                chars[(inStr.Length - 1)] = ' ';
                outStr = new string(chars);
            }
            else
            {
                outStr = inStr;
            }

            //Replace leading Underscore
            if (outStr.Length >= 2)
            {
                if (outStr[0].ToString() == "_")
                {
                    //Swap out leading "_" for " "
                    char[] chars = outStr.ToCharArray();
                    chars[0] = ' ';
                    outStr = new string(chars);
                }
            }

            return outStr;
        }

        public static string ToStringOrEmpty(this Object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}
