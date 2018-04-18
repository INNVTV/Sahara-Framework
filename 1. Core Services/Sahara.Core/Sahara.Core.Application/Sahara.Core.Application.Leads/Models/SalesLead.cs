using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Leads.Models
{
    /* Account Admin now deals with SALES LEADS DIRECTLY*/

    /// <summary>
    /// Public version of entity exposed by WCF to clients. All log results are transformed into this type.
    /// </summary>
    [DataContract]
    public class SalesLead
    {
        [DataMember]
        public string LeadID { get; set; }

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public string ProductID { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public string FullyQualifiedName { get; set; }
        [DataMember]
        public string LocationPath { get; set; }

        [DataMember]
        public string Comments { get; set; }


        [DataMember]
        public string IPAddress { get; set; }

        [DataMember]
        public string Origin { get; set; }

        [DataMember]
        public DateTimeOffset Timestamp { get; set; }


        [DataMember]
        public string Object { get; set; }

    
    }
}
