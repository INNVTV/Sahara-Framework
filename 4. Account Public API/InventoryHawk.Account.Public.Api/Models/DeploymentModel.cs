using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models
{
    public class DeploymentModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "subdomain")]
        public string Subdomain { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "environment")]
        public EnvironmentModel Environment { get; set; }

    }

    public class EnvironmentModel
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "local")]
        public string Local { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "core")]
        public string CoreServices { get; set; }
    }

}