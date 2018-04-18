using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class AccountNames
    {
        public static string RemoveSafeSpecialCharacters(string accountName)
        {
            accountName = Regex.Replace(accountName, @"\s+", ""); //<-- Converts and length of spaces, tabs, etc and replaces with a "s"
            return accountName
                .Replace(" - ", "-")
                .Replace("+", "plus")
                .Replace("!", "")
                .Replace("@", "at")
                .Replace("$", "s")
                .Replace("&", "and")
                .Replace("_", "-")
                .Replace("'", "")
                .Replace(".", "")
                .Replace("\"", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("(", "-")
                .Replace(")", "-")
                .Replace("[", "-")
                .Replace("]", "-")
                .Replace("{", "-")
                .Replace("}", "-")
                .Replace(":", "-")
                .Replace(";", "-")
                .Replace("?", "")
                .Replace("~", "-")
                .Replace("#", "")
                .Replace("%", "")
                .Replace("*", "")
                .Replace("^", "")
                .Replace("'", "")
                .Replace("`", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace(" | ", "-")
                .Replace("|", "-")
                .Replace("=", "-")
                .Replace("---------------", "-")
                .Replace("--------------", "-")
                .Replace("-------------", "-")
                .Replace("------------", "-")
                .Replace("-----------", "-")
                .Replace("----------", "-")
                .Replace("---------", "-")
                .Replace("--------", "-")
                .Replace("-------", "-")
                .Replace("------", "-")
                .Replace("-----", "-")
                .Replace("----", "-")
                .Replace("---", "-")
                .Replace("--", "-");
        }

        public static string ConvertToAccountNameKey(string accountName)
        {
            if (accountName != null)
            {
                return RemoveDiacritics(RemoveSafeSpecialCharacters(accountName).ToLower()).Replace("-", ""); ;
            }
            else
            {
                return "";
            }
        }

        public static String RemoveDiacritics(String s)
        {
            String normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }


    }
}
