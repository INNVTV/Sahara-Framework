using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    /// <summary>
    /// Generic helper class for all objects like Category Names, Tags, Etc...
    /// </summary>
    public static class PropertyNames
    {
        public static string RemoveSafeSpecialCharacters(string objectName)
        {
            objectName = Regex.Replace(objectName, @"\s+", ""); //<-- Converts and length of spaces, tabs, etc and replaces with a "-"

            return objectName
                .Replace("+", "plus")
                .Replace("!", "")
                .Replace("@", "at")
                .Replace("$", "s")
                .Replace("&", "and")
                .Replace("_", "-")
                .Replace("'", "")
                .Replace("-", "")
                .Replace("#", "")
                .Replace("\"", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("|", "")
                .Replace("?", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("~", "")
                .Replace("=", "")
                .Replace("`", "");
        }

        public static string ConvertToPropertyNameKey(string objectName, bool camelCase = false)
        {
            if (objectName != null)
            {
                if(camelCase)
                {
                    var objectNameCamelCase = Common.Methods.Strings.ToCamelCase(objectName);
                    return RemoveSafeSpecialCharacters(objectNameCamelCase);
                }
                else
                {
                    return RemoveSafeSpecialCharacters(objectName).ToLower();
                }
                
            }
            else
            {
                return "";
            }
        }
    }

}
