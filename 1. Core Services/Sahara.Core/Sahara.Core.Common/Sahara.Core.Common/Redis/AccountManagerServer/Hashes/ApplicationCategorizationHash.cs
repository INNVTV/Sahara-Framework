using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationCategorizationHash
    {
        internal static TimeSpan DefaultCacheLength = TimeSpan.FromDays(14);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":categories";
        }

        public static class Fields
        {
            #region Tree ---

            public static string Tree(bool includeHidden)
            {
                if (includeHidden) { return "tree:private"; }
                else { return "tree:public"; };
            }

            #endregion

            #region Categories ---

            public static string List(bool includeHidden)
            {
                if (includeHidden) { return "list:private"; }
                else { return "list:public";};
            }

            #endregion

            #region Category ---

            //public static string Count()
            //{
                //return "categories:count";
            //}

            public static string Category(string categoryNameKey, bool includeHiddenSubcategories)
            {
                if (includeHiddenSubcategories) {
                    return Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryNameKey) +
                        ":private";
                }
                else {
                    return Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryNameKey) +
                        ":public";
                };

            }

            #endregion

            #region Subcategory ---

            //public static string SubcategoryCount()
            //{
                //return "subcategories:count";
            //}

            public static string Subcategory(string categoryNameKey,
                string subcategoryNameKey, bool includeHidden)
            {

                if (includeHidden)
                {
                    return categoryNameKey + "/" + subcategoryNameKey + 
                        ":private";
                }
                else {
                    return categoryNameKey + "/" + subcategoryNameKey +
                        ":public";
                };
            }

            #endregion

            #region Subsubcategory ---

            //public static string SubsubcategoryCount()
            //{
                //return "subsubcategories:count";
            //}

            public static string Subsubcategory(string categoryNameKey,
                string subcategoryNameKey, string subsubcategoryNameKey, bool includeHidden)
            {

                if (includeHidden)
                {
                    return categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey +
                        ":private";
                }
                else {
                    return categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey +
                        ":public";
                };

            }

            #endregion

            #region Subsubsubcategory ---

            //public static string SubsubsubcategoryCount()
            //{
                //return "subsubsubcategories:count";
            //}

            public static string Subsubsubcategory(string categoryNameKey,
                string subcategoryNameKey, string subsubcategoryNameKey, string subsubsubcategoryNameKey)
            {
                return categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + "/" + subsubsubcategoryNameKey;
            }

            #endregion

            #region Shared

            public static string Count()
            {
                return "categorizations:count";
            }

            #endregion
        }
    }
}
