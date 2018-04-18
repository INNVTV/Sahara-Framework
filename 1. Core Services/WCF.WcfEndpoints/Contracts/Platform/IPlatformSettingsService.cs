using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Settings.Models.Partitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Platform
{
    [ServiceContract]
    public interface IPlatformSettingsService
    {
        [OperationContract]
        CorePlatformSettings GetCorePlatformSettings(string sharedClientKey);

        [OperationContract]
        string GetCurrentEnvironment();
    }



    [DataContract]
    public class CorePlatformSettings
    {
        [DataMember]
        public string ApplicationName;

        [DataMember]
        public URLSettings Urls;
        [DataMember]
        public EnvironmentSettings Environment;
        [DataMember]
        public AccountSettings Account;
        [DataMember]
        public RegsitrationSettings Registration;
        [DataMember]
        public CustodianSettings Custodian;
        [DataMember]
        public WorkerSettings Worker;
        [DataMember]
        public RedisConfigurations Redis;
        [DataMember]
        public SendGridCredentials SendGrid;
        [DataMember]
        public StorageCredentials Storage;
        [DataMember]
        public DocumentDBCredentials DocumentDB;
        //[DataMember]  (Removed in favor of new partitioning srategy)
        //public SearchCredentials Search;
        [DataMember]
        public string GoogleMapsAPIKey;

        [DataMember]
        public List<CustomDomain> CustomDomains;

        [DataMember]
        public List<StoragePartition> StorageParitions;

        [DataMember]
        public List<SearchPartition> SearchParitions;

        public CorePlatformSettings()
        {

            Urls = new URLSettings();

            Environment = new EnvironmentSettings();

            Account = new AccountSettings();

            Registration = new RegsitrationSettings();

            Custodian = new CustodianSettings();

            Worker = new WorkerSettings();

            Redis = new RedisConfigurations();

            SendGrid = new SendGridCredentials();

            Storage = new StorageCredentials();

            DocumentDB = new DocumentDBCredentials(); //<-- ReadOnly credentials for clients

            //Search = new SearchCredentials(); //<-- Client Query key for clients  (Removed in favor of new partitioning srategy)

        }

        #region Settings Classes

        [DataContract]
        public class EnvironmentSettings
        {
            [DataMember]
            public string Current = string.Empty;
        }

        [DataContract]
        public class URLSettings
        {
            //Master Domains
            [DataMember]
            public string MasterDomain = String.Empty;


            [DataMember]
            public string AccountManagementDomain = String.Empty;
            [DataMember]
            public string AccountServiceDomain = String.Empty;
            [DataMember]
            public string AccountSiteDomain = String.Empty;

            //Api Domains
            [DataMember]
            public string RegistrationApiEndpoint = String.Empty;

            //Service Domains
            [DataMember]
            public string ImagingApiEndpoint = String.Empty;

            //Other URLs
            [DataMember]
            public string RegistrationEndpoint = String.Empty;


            //AccountUser Management Links
            [DataMember]
            public string AccountUserAcceptInvitationUrl = String.Empty;
            [DataMember]
            public string AccountUserPasswordClaimUrl = String.Empty;


            //PlatformUser Management Links
            [DataMember]
            public string PlatformUserAcceptInvitationUrl = String.Empty;
            [DataMember]
            public string PlatformUserPasswordClaimUrl = String.Empty;


            //Platform Imaging Uris
            [DataMember]
            public string PlatformImagingBlobUri = String.Empty;
            [DataMember]
            public string PlatformImagingCdnUri = String.Empty;

            //Account Imaging Uris
            //[DataMember]
           // public string AccountImagingBlobUri = String.Empty;
            //[DataMember]
            //public string AccountImagingCdnUri = String.Empty;

            //Intermediary Imaging Uris
            [DataMember]
            public string IntermediaryImagingBlobUri = String.Empty;
            [DataMember]
            public string IntermediaryImagingCdnUri = String.Empty;


            //Legal, Privacy, Etc...
            [DataMember]
            public string ServiceAgreementUri = String.Empty;
            [DataMember]
            public string PrivacyPolicyUri = String.Empty;
            [DataMember]
            public string TermsAndConditionsUri = String.Empty;
            [DataMember]
            public string AcceptableUse = String.Empty;          
            [DataMember]
            public string AboutUsUri = String.Empty;
        }

        [DataContract]
        public class AccountSettings
        {

            [DataMember]
            public int TrialLength = 0;
        }

        [DataContract]
        public class RegsitrationSettings
        {
            [DataMember]
            public int PasswordMinLength = 0;

        }

        [DataContract]
        public class CustodianSettings
        {
            [DataMember]
            public string CustodianFrequencyDescription  = String.Empty;

            [DataMember]
            public int CustodianFrequencyMilliseconds = 0;
        }

        [DataContract]
        public class SendGridCredentials
        {
            //[DataMember]
            //public string UserName = String.Empty;

            [DataMember]
            public string ApiKey = String.Empty;
        }

        [DataContract]
        public class StorageCredentials
        {
            [DataMember]
            public string IntermediaryName = String.Empty;

            [DataMember]
            public string IntermediaryKey = String.Empty;

            //[DataMember]
            //public string AccountName = String.Empty;

            //[DataMember]
            //public string AccountKey = String.Empty;
        }

        [DataContract]
        public class DocumentDBCredentials
        {
            [DataMember]
            public string AccountPartitionsReadOnlyAccountName = String.Empty;

            [DataMember]
            public string AccountPartitionsReadOnlyAccountKey = String.Empty;

            //[DataMember]
            //public string AccountPartitionsDatabaseSelfLink = String.Empty;

            [DataMember]
            public string AccountPartitionsDatabaseId = String.Empty;
        }

        /*  (Removed in favor of new partitioning srategy)
        [DataContract]
        public class SearchCredentials
        {
            [DataMember]
            public string SearchServiceName = String.Empty;

            [DataMember]
            public string ClientQueryKey = String.Empty;

        }
        */

        [DataContract]
        public class WorkerSettings
        {
            [DataMember]
            public string WorkerFrequencyDescription = String.Empty;

            [DataMember]
            public int WorkerFrequencyMilliseconds = 0;
        }

        [DataContract]
        public class RedisConfigurations
        {

            [DataMember]
            public string Unsecure = String.Empty;


            //[DataMember]
            //public string PlatformManager_Secure = String.Empty;
            //[DataMember]
            //public string PlatformManager_Unsecure = String.Empty;



            //[DataMember]
            //public string AccountManager_Secure = String.Empty;
            //[DataMember]
            //public string AccountManager_Unsecure = String.Empty;

        }


        //Used to send a list of ALL custom domains for all accounts in the system to the main website
        [DataContract]
        public class CustomDomain
        {

            [DataMember]
            public string AccountNameKey = String.Empty;
            [DataMember]
            public string Domain = String.Empty;

        }

        #endregion



    }
}
