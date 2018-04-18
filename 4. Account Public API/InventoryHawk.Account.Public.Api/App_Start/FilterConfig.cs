using System.Web;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
