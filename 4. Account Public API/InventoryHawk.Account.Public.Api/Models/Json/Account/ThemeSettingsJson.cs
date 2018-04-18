using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Account
{
    public class ThemeSettingsJson
    {
        public string name { get; set; }

        public ThemeColorsJson colors { get; set; }
        public string font { get; set; }

        public ThemeSettingsJson()
        {
            colors = new ThemeColorsJson();
        }
    }

    public class ThemeColorsJson
    {

        public string background { get; set; }
        public string backgroundGradientTop { get; set; }
        public string backgroundGradientBottom { get; set; }


        public string shadow { get; set; }
        public string highlight { get; set; }

        public string foreground { get; set; }

        public string overlay { get; set; }

        public string trim { get; set; }

    }

}