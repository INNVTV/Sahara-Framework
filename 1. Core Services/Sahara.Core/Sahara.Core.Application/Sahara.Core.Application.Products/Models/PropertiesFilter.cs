using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.Models
{
    [DataContract]
    public class PropertyFilter
    {
        [DataMember]
        public Dictionary<string, string> PropertyValues;
    }
}
