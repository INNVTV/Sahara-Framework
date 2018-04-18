using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Services.CloudFlare.Models
{
    class DnsListingResponse
    {
        public List<DnsRecord> result { get; set; }
        public bool success { get; set; }

    }

    class DnsRecord
    {
        public string id { get; set; }
        public string name { get; set; }        //<--xxx.xxx.com
        public string type { get; set; }        //<--A,CNAME,TXT, etc...
        public string content { get; set; }     //<--xxx.azurewebsites.net
    }
}
