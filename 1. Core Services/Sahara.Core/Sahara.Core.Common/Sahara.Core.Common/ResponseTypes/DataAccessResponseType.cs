using Sahara.Core.Common.Codes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sahara.Core.Common.ResponseTypes
{
    
    [DataContract]
    public class DataAccessResponseType
    {

        public DataAccessResponseType()
        {
            ErrorMessages = new List<string>();
            //SuccessMessages = new List<string>();
            //Results = new List<string>();
            isSuccess = false;
            //ErrorCode = 0;

            RoleInstance = Settings.Azure.CurrentRoleInstance.Id;
            RoleName = Settings.Azure.CurrentRoleInstance.Name;

        }

        [DataMember]
        public bool isSuccess { get; set; }

        [DataMember]
        public string SuccessCode { get; set; }

        [DataMember]
        public string SuccessMessage { get; set; }

        [DataMember]
        public List<String> SuccessMessages { get; set; }

        [DataMember]
        public List<String> Results { get; set; }

        //---------------------------------------------------

        [DataMember]
        public int RoleInstance { get; set; }
        [DataMember]
        public string RoleName { get; set; }

        //---------------------------------------------------

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public List<String> ErrorMessages { get; set; }


        //[DataMember] // Leaving off allows for WCFTestClient.exe to get results of this type. testClient will choke on generic Object type. If necesary extend this class and use specific classes for response object results
        public Object ResponseObject { get; set; }
    }

    /*
    public class GetUserWithLogin_DataAccessResponseType : DataAccessResponseType
    {
        [DataMember]
        public PlatformUserIdentity PlatformUserIdentity { get; set; }
    }*/
}
