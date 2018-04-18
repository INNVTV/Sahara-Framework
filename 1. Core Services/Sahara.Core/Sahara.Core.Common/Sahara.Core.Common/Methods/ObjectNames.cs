using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    /// <summary>
    /// Generic helper class for all objects like Category Names, Tags, Etc...
    /// </summary>
    public static class ObjectNames
    {
        public static string RemoveSafeSpecialCharacters(string objectName)
        {
            objectName = Regex.Replace(objectName, @"\s+", "-"); //<-- Converts and length of spaces, tabs, etc and replaces with a "-"

            return objectName
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

        public static string ConvertToObjectNameKey(string objectName)
        {
            if (objectName != null)
            {
                var result = RemoveDiacritics(RemoveSafeSpecialCharacters(objectName).ToLower());

                //Remove any trailing dash
                if(result.EndsWith("-"))
                {
                    result = result.Substring(0, result.Length - 1);
                }

                return result.TrimEnd(); //<-- Trims whitespace
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
