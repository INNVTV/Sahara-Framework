using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Customers.Models
{
    [DataContract]
    public class CustomerModel
    {
        [DataMember]
        public Guid CustomerID;

        [DataMember]
        public string FirstName;
        [DataMember]
        public string MiddleName;
        [DataMember]
        public string LastName;
        [DataMember]
        public string NickName;


        //[DataMember]
        //public List<String> Products; //<-- Table storage reference model of products interested in or purchased

        //[DataMember]
        //public List<Addresses> Addresses; //<-- Table storage reference model of addresses (we show a map in ui)

        //[DataMember]
        //public List<Social> SocialAccounts; //<-- Table storage reference model of addresses (we show a map in ui)


        [DataMember]
        public bool Active;

        [DataMember]
        public DateTime CreatedDate;
    }
}
