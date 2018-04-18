using Sahara.Core.Accounts.Registration.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Validation.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountRegistrationService
    {
        /*==================================================================================
         * Account Registration Service 
         ==================================================================================*/ 

        [OperationContract]
        DataAccessResponseType RegisterAccount(RegisterNewAccountModel registerNewAccountModel, string sharedClientKey);


        /*==================================================================================
         * Validation Registration Services 
         ==================================================================================*/ 
        
        [OperationContract]
        ValidationResponseType ValidateAccountName(string AccountName);

        [OperationContract]
        ValidationResponseType ValidateEmail(string Email);

        [OperationContract]
        ValidationResponseType ValidatePhoneNumber(string PhoneNumber);

        [OperationContract]
        ValidationResponseType ValidateFirstName(string FirstName);

        [OperationContract]
        ValidationResponseType ValidateLastName(string LastName);


        /*==================================================================================
        * PROVISIONING Methods for Account(s)
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType ProvisionAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);


        /*==================================================================================
        * Helper Methods
        ==================================================================================*/

        [OperationContract]
        string ConvertToAccountNameKey(string AccountName);


    }



}
