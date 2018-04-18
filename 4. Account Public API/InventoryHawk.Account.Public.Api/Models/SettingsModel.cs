using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models
{
    public class SettingsModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "display")]
        public SettingsDisplayModel Display { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "theme")]
        public ThemeModel Theme { get; set; }
    }

    public class SettingsDisplayModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "lists")]
        public SettingsDisplayListsModel Lists { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "details")]
        public SettingsDisplayDetailsModel Details { get; set; }
    }

    public class SettingsDisplayListsModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "showprice")]
        public bool ShowPrice { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "showtitle")]
        public bool ShowTitle { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "showsubtitle")]
        public bool ShowSubTitle { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "showdescription")]
        public bool ShowDescription { get; set; }

    }

    public class SettingsDisplayDetailsModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "showprice")]
        public bool ShowPrice { get; set; }
    }

}