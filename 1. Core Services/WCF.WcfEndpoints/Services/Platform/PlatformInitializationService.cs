using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Platform.Initialization;

namespace WCF.WcfEndpoints.Service.Platform
{
    public class PlatformInitializationService : WCF.WcfEndpoints.Contracts.Platform.IPlatformInitializationService
    {
        public bool IsPlatformInitialized()
        {
            return PlatformInitializationManager.isPlatformInitialized(); 
        }

        public Sahara.Core.Common.ResponseTypes.DataAccessResponseType ProvisionPlatform(string FirstName, string LastName, string Email, string Password)
        {
            return PlatformInitializationManager.ProvisionPlatform(FirstName, LastName, Email, Password);
        }

        public Sahara.Core.Common.ResponseTypes.DataAccessResponseType PurgePlatform()
        {
            return PlatformInitializationManager.PurgePlatform();
        }
    }
}
