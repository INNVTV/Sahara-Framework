using InventoryHawk.Account.Public.Api.ApplicationImageFormatsService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class ImageFormatsController : Controller
    {

        // GET: ImageFormats
        public ActionResult Index()
        {
            return View();
        }
    }
}