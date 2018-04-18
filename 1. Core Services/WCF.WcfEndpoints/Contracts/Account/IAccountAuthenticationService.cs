using Sahara.Core.Accounts.Models;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountAuthenticationService
    {
        [OperationContract]
        AuthenticationResponse Authenticate(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey);

        [OperationContract]
        ClaimsIdentity GetClaimsIdentity(string userName, string sharedClientKey);

        [OperationContract]
        void Logout(string userName, string sharedClientKey);
    }



    [DataContract]
    public class AuthenticationResponse
    {
        [DataMember]
        public bool isSuccess;

        [DataMember]
        public string ErrorMessage;

        [DataMember]
        public AccountUser AccountUser;

        [DataMember]
        public ClaimsIdentity ClaimsIdentity;
    }
}
