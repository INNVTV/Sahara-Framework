using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models
{
    //Move to CoreService:
    public class ThemeModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "colors")]
        public ThemeColorsModel Colors { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "font")]
        public ThemeFontModel Font { get; set; }

        public ThemeModel()
        {
            Colors = new ThemeColorsModel();
        }
    }

    public class ThemeColorsModel
    {

        [Newtonsoft.Json.JsonProperty(PropertyName = "background")]
        public string Background { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "backgroundgradienttop")]
        public string BackgroundGradientTop { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "backgroundgradientbottom")]
        public string BackgroundGradientBottom { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "shadow")]
        public string Shadow { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "highlight")]
        public string Highlight { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "foreground")]
        public string Foreground { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "overlay")]
        public string Overlay { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "trim")]
        public string Trim { get; set; }

        /*
        [Newtonsoft.Json.JsonProperty(PropertyName = "active")]
        public string Active { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "inactive")]
        public string Inactive { get; set; }
        */
    }

    public class ThemeFontModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /*
        [Newtonsoft.Json.JsonProperty(PropertyName = "header")]
        public string Header { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "Subject")]
        public string Subject { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "menu")]
        public string Menu { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "footer")]
        public string Footer { get; set; }
        */
    }
}