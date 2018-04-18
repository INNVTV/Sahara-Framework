using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatformAdminSite.Models.Infrastructure
{

    public class RedisServerModel
    {
        public RedisServerModel()
        {
            StringKeys = new List<string>();
            HashKeys = new List<string>();
        }

        public string Name { get; set; }

        public List<string> StringKeys { get; set; }
        public List<string> HashKeys { get; set; }
    }
}