using Sahara.Core.Platform.Users.Models;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;

namespace WCF.WcfEndpoints.Contracts.Platform
{
    [ServiceContract]
    public interface IPlatformAuthenticationService
    {
        [OperationContract]
        AuthenticationResponse Authenticate(string email, string password, string ipAddress, string origin, string sharedClientKey);

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
        public PlatformUser PlatformUser;

        [DataMember]
        public ClaimsIdentity ClaimsIdentity;
    }
}
