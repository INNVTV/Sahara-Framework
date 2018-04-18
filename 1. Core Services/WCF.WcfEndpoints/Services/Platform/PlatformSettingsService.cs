using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Settings;
using Sahara.Core.Platform.Partitioning.Public;
using WCF.WcfEndpoints.Contracts.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WCF.WcfEndpoints.Service.Platform
{
    public class PlatformSettingsService : WCF.WcfEndpoints.Contracts.Platform.IPlatformSettingsService
    {
        public CorePlatformSettings GetCorePlatformSettings(string sharedClientKey)
        {

            // Ensure the clients are certified.
            if(sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }


            var corePlatformSettings = new CorePlatformSettings();

            //Set ApplicationName:
            corePlatformSettings.ApplicationName = Sahara.Core.Settings.Application.Name;

            //Set Urls:
            corePlatformSettings.Urls.MasterDomain = Sahara.Core.Settings.Endpoints.URLs.MasterDomain;

            //Google Maps API Key
            corePlatformSettings.GoogleMapsAPIKey = Sahara.Core.Settings.Services.GoogleMaps.Account.ApiKey;

            //Account domains
            corePlatformSettings.Urls.AccountManagementDomain = Sahara.Core.Settings.Endpoints.URLs.AccountManagementDomain;
            corePlatformSettings.Urls.AccountServiceDomain = Sahara.Core.Settings.Endpoints.URLs.AccountServiceDomain;
            corePlatformSettings.Urls.AccountSiteDomain = Sahara.Core.Settings.Endpoints.URLs.AccountSiteDomain;

            corePlatformSettings.Urls.RegistrationApiEndpoint = Sahara.Core.Settings.Endpoints.URLs.RegistrationApiEndpoint;
            corePlatformSettings.Urls.ImagingApiEndpoint = Sahara.Core.Settings.Endpoints.URLs.ImagingApiUrl;

            corePlatformSettings.Urls.RegistrationEndpoint = Sahara.Core.Settings.Endpoints.URLs.RegistrationUrl;

            corePlatformSettings.Urls.AccountUserAcceptInvitationUrl = Sahara.Core.Settings.Endpoints.URLs.AccountUserAcceptInvitationUrl;
            corePlatformSettings.Urls.AccountUserPasswordClaimUrl = Sahara.Core.Settings.Endpoints.URLs.AccountUserPasswordClaimUrl;

            corePlatformSettings.Urls.PlatformUserAcceptInvitationUrl = Sahara.Core.Settings.Endpoints.URLs.PlatformUserAcceptInvitationUrl;
            corePlatformSettings.Urls.PlatformUserPasswordClaimUrl = Sahara.Core.Settings.Endpoints.URLs.PlatformUserPasswordClaimUrl;

            //Imaging Urls
            corePlatformSettings.Urls.PlatformImagingBlobUri = Sahara.Core.Settings.Imaging.Images.PlatformBlobUri;
            corePlatformSettings.Urls.PlatformImagingCdnUri = Sahara.Core.Settings.Imaging.Images.PlatformCdnUri;



            //Legal, Privacy, Etc...
            corePlatformSettings.Urls.ServiceAgreementUri = Sahara.Core.Settings.Endpoints.URLs.ServiceAgreement;
            corePlatformSettings.Urls.PrivacyPolicyUri = Sahara.Core.Settings.Endpoints.URLs.PrivacyPolicy;
            corePlatformSettings.Urls.TermsAndConditionsUri = Sahara.Core.Settings.Endpoints.URLs.TermsAndConditions;
            corePlatformSettings.Urls.AcceptableUse = Sahara.Core.Settings.Endpoints.URLs.AcceptableUse;
            corePlatformSettings.Urls.AboutUsUri = Sahara.Core.Settings.Endpoints.URLs.AboutUs;

            //corePlatformSettings.Urls.AccountImagingBlobUri = Sahara.Core.Settings.Imaging.Images.AccountBlobUri;
            //corePlatformSettings.Urls.AccountImagingCdnUri = Sahara.Core.Settings.Imaging.Images.AccountCdnUri;

            corePlatformSettings.Urls.IntermediaryImagingBlobUri = Sahara.Core.Settings.Imaging.Images.IntermediaryBlobUri;
            corePlatformSettings.Urls.IntermediaryImagingCdnUri = Sahara.Core.Settings.Imaging.Images.IntermediaryCdnUri;

            //Set Environment:
            corePlatformSettings.Environment.Current = Sahara.Core.Settings.Environment.Current; 

            //Set Registration Variables:
            corePlatformSettings.Registration.PasswordMinLength = Sahara.Core.Settings.Accounts.Registration.PasswordMinimumLength;
            

            //Set Interediate Storage Account Access:
            corePlatformSettings.Storage.IntermediaryName = Sahara.Core.Settings.Azure.Storage.StorageConnections.IntermediaryStorageName;
            corePlatformSettings.Storage.IntermediaryKey = Sahara.Core.Settings.Azure.Storage.StorageConnections.IntermediaryStorageKey;

            //Set Account Storage Account Access:
            //corePlatformSettings.Storage.AccountName = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountStorageName;
            //corePlatformSettings.Storage.AccountKey = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountStorageKey;

            //Set Custodian Variables:
            corePlatformSettings.Custodian.CustodianFrequencyDescription = Sahara.Core.Settings.Platform.Custodian.Frequency.Description;
            corePlatformSettings.Custodian.CustodianFrequencyMilliseconds = Sahara.Core.Settings.Platform.Custodian.Frequency.Length;

            //Set Worker Variables:
            corePlatformSettings.Worker.WorkerFrequencyDescription = Sahara.Core.Settings.Platform.Worker.EasebackRound3.duration;
            corePlatformSettings.Worker.WorkerFrequencyMilliseconds = Sahara.Core.Settings.Platform.Worker.EasebackRound3.milliseconds;

            //Set Redis Configurations

            corePlatformSettings.Redis.Unsecure = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.Configuration; //Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration.ToString();
            //var securePlatformManagerConnection = Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration;
            //securePlatformManagerConnection.ssl = true;
            //corePlatformSettings.Redis.PlatformManager_Secure = securePlatformManagerConnection.ToString();


            //corePlatformSettings.Redis.PlatformManager_Unsecure = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.Configuration; //Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration.ToString();
            //var securePlatformManagerConnection = Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration;
            //securePlatformManagerConnection.ssl = true;
            //corePlatformSettings.Redis.PlatformManager_Secure = securePlatformManagerConnection.ToString();


            //corePlatformSettings.Redis.AccountManager_Unsecure = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.Configuration; //Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration.ToString();
            //var secureAccountManagerConnection = Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration;
            //secureAccountManagerConnection.ssl = true;
            //corePlatformSettings.Redis.AccountManager_Secure = secureAccountManagerConnection.ToString();

            //Set SendGrid Credentials:
            //corePlatformSettings.SendGrid.UserName = Sahara.Core.Settings.Services.SendGrid.Account.UserName;
            corePlatformSettings.SendGrid.ApiKey = Sahara.Core.Settings.Services.SendGrid.Account.APIKey;

            //DocumentDB ReadOnly Credentials
            corePlatformSettings.DocumentDB.AccountPartitionsReadOnlyAccountName = Sahara.Core.Settings.Azure.DocumentDB.ReadOnlyAccountName;
            corePlatformSettings.DocumentDB.AccountPartitionsReadOnlyAccountKey = Sahara.Core.Settings.Azure.DocumentDB.ReadOnlyAccountKey;
            //corePlatformSettings.DocumentDB.AccountPartitionsDatabaseSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            corePlatformSettings.DocumentDB.AccountPartitionsDatabaseId = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId;

            //Azure Search Readonly Credentials (Removed in favor of new partitioning srategy)
            //corePlatformSettings.Search.SearchServiceName = Sahara.Core.Settings.Azure.Search.AccountsServiceName;
            //corePlatformSettings.Search.ClientQueryKey = Sahara.Core.Settings.Azure.Search.AccountsClientKey;

            //Account Custom Domains
            corePlatformSettings.CustomDomains = GetCustomDomains();

            //Storage Partitions (with read only keys)
            corePlatformSettings.StorageParitions = StoragePartitioningManager.GetStoragePartitions();

            //Search Partitions (with read only keys)
            corePlatformSettings.SearchParitions = SearchPartitioningManager.GetSearchPartitions();

            return corePlatformSettings;
        }

        public string GetCurrentEnvironment()
        {
            return Sahara.Core.Settings.Environment.Current;
        }

        public List<CorePlatformSettings.CustomDomain> GetCustomDomains()
        {
            var customDomains = new List<CorePlatformSettings.CustomDomain>();

            //Get all accounts
            var accounts = AccountManager.GetAllAccountsByFilter("Active", "1", 0, 5000, "AccountNameKey");

            //Get settings for each account
            foreach(var account in accounts)
            {
                if(account.Provisioned)
                {
                    try
                    {
                        var settings = AccountSettingsManager.GetAccountSettings(account);

                        if (settings.CustomDomain != null)
                        {
                            if (settings.CustomDomain != "")
                            {
                                customDomains.Add(new CorePlatformSettings.CustomDomain { AccountNameKey = account.AccountNameKey, Domain = settings.CustomDomain });
                            }
                        }
                    }
                    catch
                    {

                    }
                }

            }

            return customDomains;
        }
    }
}
