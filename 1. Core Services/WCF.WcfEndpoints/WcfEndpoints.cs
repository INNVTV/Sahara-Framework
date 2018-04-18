using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using WCF.WcfEndpoints.Contracts.Account;
using WCF.WcfEndpoints.Service.Account;
using WCF.WcfEndpoints.Contracts.Platform;
using WCF.WcfEndpoints.Service.Platform;
using WCF.WcfEndpoints.Contracts.Application;
using WCF.WcfEndpoints.Service.Application;
using WCF.WcfEndpoints.Contracts.Infrastructure;
using WCF.WcfEndpoints.Service.Infrastructure;

namespace WCF.WcfEndpoints
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WcfEndpoints : StatelessService
    {

        private static string wcfDomain = "localhost";

        // You'll need to set up 2 LBRules and 2 LBHealthProbes (see: "\_documentation\sf-loadbalancer-settings" folder for sample images)
        // 1 for each port (8080/8081)
        private const string listenerPort = "8080";
        private const string metaExchangePort = "8081";


        public WcfEndpoints(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {

            #region Set wcfDomain based on current enviornment

            switch (Sahara.Core.Settings.Environment.Current)
            {
                case "local":
                    wcfDomain = "localhost";
                    break;

                case "debug":
                    wcfDomain = "localhost";
                    break;

                case "stage":
                    wcfDomain = "[Config_WCFName]-stage.westus2.cloudapp.azure.com";
                    break;

                case "production":
                    wcfDomain = "[Config_WCFName].westus2.cloudapp.azure.com";
                    break;

                default:
                    wcfDomain = "localhost";
                    break;
            }

            #endregion

            return new[]
            {

                // ACCOUNT Endpoints --------

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountAuthentication,
                    "AccountAuthentication"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountManagement,
                    "AccountManagement"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountRegistration,
                    "AccountRegistration"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountCommunication,
                    "AccountCommunication"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountPaymentPlan,
                    "AccountPaymentPlan"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountBilling,
                    "AccountBilling"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForAccountCommerce,
                    "AccountCommerce"
                    ),

                // APPLICATION Endpoints --------

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationApiKeys,
                    "ApplicationApiKeys"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationProduct,
                    "ApplicationProduct"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationLeads,
                    "ApplicationLeads"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationCategorization,
                    "ApplicationCategorization"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationProperties,
                    "ApplicationProperties"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationSearch,
                    "ApplicationSearch"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationTags,
                    "ApplicationTags"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationImageFormats,
                    "ApplicationImageFormats"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationImageRecords,
                    "ApplicationImageRecords"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationImages,
                    "ApplicationImages"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForApplicationDataInjection,
                    "ApplicationDataInjection"
                    ),

                // INFRASTRUCTURE Endpoints --------

                new ServiceInstanceListener(
                    this.CreateWCFListenerForInfrastructureTests,
                    "Infrastructure"
                    ),

                // PLATFORM Endpoints --------

                new ServiceInstanceListener(
                    this.CreateWCFListenerForPlatformManagement,
                    "PlatformManagement"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForPlatformBilling,
                    "PlatformBilling"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForPlatformInitialization,
                    "PlatformInitialization"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForPlatformSettings,
                    "PlatformSettings"
                    ),

                new ServiceInstanceListener(
                    this.CreateWCFListenerForPlatformAuthentication,
                    "PlatformAuthentication"
                    ),





            };
        }


        #region Listener Creation

        #region Account

        private ICommunicationListener CreateWCFListenerForAccountAuthentication(StatelessServiceContext context)
        {
            var servicePath = "Account/Authentication";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountAuthenticationService>(
                context,
                new AccountAuthenticationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountManagement(StatelessServiceContext context)
        {
            var servicePath = "Account/Management";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 6000000); //<-- Set to 6mb for profile images

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountManagementService>(
                context,
                new AccountManagementService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountRegistration(StatelessServiceContext context)
        {
            var servicePath = "Account/Registration";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountRegistrationService>(
                context,
                new AccountRegistrationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountCommunication(StatelessServiceContext context)
        {
            var servicePath = "Account/Communication";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountCommunicationService>(
                context,
                new AccountCommunicationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountPaymentPlan(StatelessServiceContext context)
        {
            var servicePath = "Account/PaymentPlan";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountPaymentPlanService>(
                context,
                new AccountPaymentPlanService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountBilling(StatelessServiceContext context)
        {
            var servicePath = "Account/Billing";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountBillingService>(
                context,
                new AccountBillingService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForAccountCommerce(StatelessServiceContext context)
        {
            var servicePath = "Account/Commerce";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IAccountCommerceService>(
                context,
                new AccountCommerceService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        #endregion


        #region Application

        private ICommunicationListener CreateWCFListenerForApplicationApiKeys(StatelessServiceContext context)
        {
            var servicePath = "Application/ApiKeys";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationApiKeysService>(
                context,
                new ApplicationApiKeysService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationProduct(StatelessServiceContext context)
        {
            var servicePath = "Application/Product";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationProductService>(
                context,
                new ApplicationProductService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationLeads(StatelessServiceContext context)
        {
            var servicePath = "Application/Leads";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationLeadsService>(
                context,
                new ApplicationLeadsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationCategorization(StatelessServiceContext context)
        {
            var servicePath = "Application/Categorization";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationCategorizationService>(
                context,
                new ApplicationCategorizationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationProperties(StatelessServiceContext context)
        {
            var servicePath = "Application/Properties";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 6000000); //<-- Set to 6mb for swatch images

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationPropertiesService>(
                context,
                new ApplicationPropertiesService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationSearch(StatelessServiceContext context)
        {
            var servicePath = "Application/Search";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationSearchService>(
                context,
                new ApplicationSearchService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationTags(StatelessServiceContext context)
        {
            var servicePath = "Application/Tags";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationTagsService>(
                context,
                new ApplicationTagsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationImageFormats(StatelessServiceContext context)
        {
            var servicePath = "Application/ImageFormats";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationImageFormatsService>(
                context,
                new ApplicationImageFormatsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationImageRecords(StatelessServiceContext context)
        {
            var servicePath = "Application/ImageRecords";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationImageRecordsService>(
                context,
                new ApplicationImageRecordsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationImages(StatelessServiceContext context)
        {
            var servicePath = "Application/Images";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationImagesService>(
                context,
                new ApplicationImagesService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForApplicationDataInjection(StatelessServiceContext context)
        {
            var servicePath = "Application/DataInjection";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IApplicationDataInjectionService>(
                context,
                new ApplicationDataInjectionService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        #endregion

        #region Infrastructure

        private ICommunicationListener CreateWCFListenerForInfrastructureTests(StatelessServiceContext context)
        {
            var servicePath = "Infrastructure/Tests";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IInfrastructureTestsService>(
                context,
                new InfrastructureTestsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        #endregion

        #region Platform

        private ICommunicationListener CreateWCFListenerForPlatformManagement(StatelessServiceContext context)
        {
            var servicePath = "Platform/Management";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 6000000); //<-- Set to 6mb for profile images

            //((System.ServiceModel.NetTcpBinding)bindings).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties


            var listener = new WcfCommunicationListener<IPlatformManagementService>(
                context,
                new PlatformManagementService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);


            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForPlatformBilling(StatelessServiceContext context)
        {
            var servicePath = "Platform/Billing";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IPlatformBillingService>(
                context,
                new PlatformBillingService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForPlatformInitialization(StatelessServiceContext context)
        {
            var servicePath = "Platform/Initialization";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IPlatformInitializationService>(
                context,
                new PlatformInitializationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForPlatformSettings(StatelessServiceContext context)
        {
            var servicePath = "Platform/Settings";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IPlatformSettingsService>(
                context,
                new PlatformSettingsService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }

        private ICommunicationListener CreateWCFListenerForPlatformAuthentication(StatelessServiceContext context)
        {
            var servicePath = "Platform/Authentication";
            var serviceUri = String.Format("net.tcp://{0}:{1}/Services/{2}", wcfDomain, listenerPort, servicePath);
            var metaExchangeUri = String.Format("net.tcp://{0}:{1}/Services/{2}/mex", wcfDomain, metaExchangePort, servicePath);

            var bindings = WcfUtility.CreateTcpListenerBinding(maxMessageSize: 2500000); //<-- Set to 2.5mb for larger packages

            //((System.ServiceModel.NetTcpBinding)binding).MaxReceivedMessageSize = 6000000 <-- Cast as System.ServiceModel.NetTcpBinding to access other properties

            var listener = new WcfCommunicationListener<IPlatformAuthenticationService>(
                context,
                new PlatformAuthenticationService(),
                bindings,
                new EndpointAddress(serviceUri));
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

            listener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            listener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, metaExchangeUri, new Uri(metaExchangeUri));

            listener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            listener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });

            return listener;
        }


        #endregion


        #endregion

        #region Depricated Tests

        // ------------------------ ORG ATTEMPT --------------------------


        /*

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {

            #region create shared endpoint variables/objects

            ////<Endpoint Protocol="tcp" Name="WcfServiceEndpoint" Type="Input" Port="8081" />
            string serviceEndpointName = "WcfServiceEndpoint";     //<-- Same as TCP Endpoint Name from "ServiceManifest.xml":



            //TO DO DETERMINE ROLE ENVIORNMENT:
            //string enviornment = RoleEnvironment.GetConfigurationSettingValue("Environment");
            string enviornment = "local";

            //Get domain name from configuration file (Local, Debug, Stage or Release):
            string domainNameFixed = Settings.ServiceEndpoint.GetEndpoint(enviornment);



            //This is the "Public port" of the "wsfTcpPort" input endpoint configured in ServiceDefinition.csdf
            //It is exposed by the loadbalancer on the CloudService.
            //Value MUST be same as configured in ServiceDefinition.csdf
            //int wcfTcpPortFixed = 8081;

            //Each role instance will have a dynamic ip address, and a dynamic tcp port that the WCF service needs to listen on.
            //Value of endpoint name "WcfTcpPort" MUST be same as the port name configured in ServiceDefinition.csdf
            //string ipAddressDynamic = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WcfTcpPort"].IPEndpoint.Address.ToString();
            //int wcfTcpPortDynamic = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WcfTcpPort"].IPEndpoint.Port;

            //Updated for Service Fabric -----------
            //"Fabric_NodeIPOrFQDN=containerhost1",
            //"Fabric_NodeName=host1",
            //"Fabric_RuntimeConnectionAddress=localhost:19003",
            //Each service instance will have a dynamic ip address, and a dynamic tcp port that the WCF service needs to listen on.
            string ipAddressDynamic = Environment.GetEnvironmentVariable("Fabric_NodeIPOrFQDN");
            int wcfTcpPort = Settings.ServicePorts.GetServicePort(enviornment);
            //string nodeIpAndPortAddress = Environment.GetEnvironmentVariable("Fabric_RuntimeConnectionAddress");

            //Create a TCP binding that will be used to communicate directly over TCP with NO SECURITY required
            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);

            // We quadruple the MaxReceivedMessageSize of 65536 for large calls like for logs, etc...
            tcpBinding.MaxReceivedMessageSize = 5000000; //<-- Increased to 5mb for Profile Photo Uploads (default is 65536 bytes). This bound on message size is intended to limit exposure to denial of service (DoS) attacks.
            tcpBinding.MaxBufferSize = 5000000; //<-- Increased to 5mb for Profile Photo Uploads (default is 65536 bytes). If you receive more data than you can buffer, the data that exceeds the buffer size remains on the underlying socket until your buffer has room for the rest of the data.
            tcpBinding.MaxBufferPoolSize = 0; //<-- Set to 0 for large messages. Better for Garbage Collection: For larger messages, it’s often best to avoid pooling altogether, which you can accomplish by setting MaxBufferPoolSize=0. You should of course profile your code under different values to determine the settings that will be optimal for your application. 

            //    Used toUpdate the ServiceHost object to allow for MetadataExchange Behaviour
            //    Allows WCFTestClient & VS > Add ServiceReference to download service descrition
            //    If removed, clients must have a copy of the Contracts DLL
            ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();




            #endregion



            #region Create Communication Listeners

            #region Listener 1

            // 1. Create the Listener Object
            var testCommunicationListener1 = new WcfCommunicationListener<IContract1>(
                   this.Context,
                   new Service1(),
                   WcfUtility.CreateTcpClientBinding(),
                   serviceEndpointName
               );

            testCommunicationListener1.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            // 3. Add service debug information to include exception details in faults
            testCommunicationListener1.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            testCommunicationListener1.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });


            #region Endpoint Generation

            //TO DO Inject Contract Type:
            Type contractInterfaceType = typeof(IContract1);
            //Type contractType; <-- Not used


            //TO DO Inject Service Path Name:
            string servicePath = Settings.ServicePaths.TestServicePath;

            //Exceptions:
            if (servicePath == "/Services/Platform/Initialization")
            {
                //For the initialization process we increase the timeout to 5 min.
                tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            }

            if (servicePath == "/Services/Application/Images")
            {
                //For the image processing we increase the timeout to 2.5 min.
                tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(2.5);
            }


            //tcpBinding.MaxConnections = 20; //<-- Default is 10: Gets or sets a value that controls the maximum number of connections to be pooled for subsequent reuse on the client and the maximum number of connections allowed to be pending dispatch on the server.
            //http://stackoverflow.com/questions/3689008/wcf-maxconnections-property

            //Define the FIXED URL that client's will use to communicate with the service.
            // will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath]
            string serviceEndpointUrl = string.Format("{0}{1}", domainNameFixed, servicePath);

            //Create a UNIQUE URL for the service endpoint given this role instance's dynamic ip address and port:
            //(each load balanced instance will have it's own)
            // will be something like: net.tcp://[111.22.33.444]:[port]/[servicePath]
            //string serviceListenUrl = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPortDynamic, servicePath);
            string serviceListenUrl = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPort, servicePath);

            //When creating multiple listneres url must be unique
            testCommunicationListener1.ServiceHost.Description.Endpoints[0].Address = new EndpointAddress(serviceListenUrl);

            #endregion


            //Create the fixed endpoint for the metadata exchange ("mex") that client's will use:
            //will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath]/mex
            string mexEndpointUrl = string.Format("{0}{1}/mex", domainNameFixed, servicePath);

            //Create the URL that this role instance actually listens on to give it's dynamic ip address & port:
            //will be something like: net.tcp://[111.22.333.44]:[port]/[servicePath]/mex
            //string mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPortDynamic, servicePath);
            string mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, servicePath);


            //Upadted 
            //mexEndpointUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");
            //mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");

            // 3. //Create the mex endpoint, exposing the IMetadataExchange contract over the mex tcp binding, on the Urls we just built:
            // Used to allow MexTcpBinding to allow metadata to be exchanged over tcp
            Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            mexBinding.Name = "Binding1"; //<--Mex binding names must be unique to avoid collisions
            testCommunicationListener1.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, mexEndpointUrl, new Uri(mexListenUrl));

            //testCommunicationListener.ServiceHost.Description.Endpoints[2].Name = "ServicesTestsmex";

            //testCommunicationListener.ServiceHost.Authentication.AuthenticationSchemes = new System.Net.AuthenticationSchemes();


            // 4. Log the creation

            //ServiceEventSource.Current.ServiceMessage(this.Context, serviceName + " Services (From WCF Core Services) are listening on:");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath + " (From WCF Core Services) on: " + serviceEndpointUrl + "(endpoint)");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath + " Services (From WCF Core Services) on: " + serviceListenUrl + "(listener)");

            //Remove if MetadataExchangeBinding is not being provided:
            ServiceEventSource.Current.ServiceMessage(this.Context, "--------");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath + " Services (From WCF Core Services) on: " + mexEndpointUrl + "(mex endpoint)");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath + " Services (From WCF Core Services) on: " + mexListenUrl + "(mex listener)");

            #endregion

            #region Listener 2

            // 1. Create the Listener Object
            var testCommunicationListener2 = new WcfCommunicationListener<IContract2>(
                this.Context,
                new Service2(),
                WcfUtility.CreateTcpClientBinding(),
                serviceEndpointName
            );



            testCommunicationListener2.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

            // 3. Add service debug information to include exception details in faults
            testCommunicationListener2.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            testCommunicationListener2.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });


            #region Endpoint Generation

            //TO DO Inject Contract Type:
            Type contractInterfaceType2 = typeof(IContract2);
            //Type contractType; <-- Not used


            //TO DO Inject Service Path Name:
            string servicePath2 = Settings.ServicePaths.TestServicePath2;

            //Exceptions:
            if (servicePath2 == "/Services/Platform/Initialization")
            {
                //For the initialization process we increase the timeout to 5 min.
                tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);
            }

            if (servicePath2 == "/Services/Application/Images")
            {
                //For the image processing we increase the timeout to 2.5 min.
                tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(2.5);
            }


            //tcpBinding.MaxConnections = 20; //<-- Default is 10: Gets or sets a value that controls the maximum number of connections to be pooled for subsequent reuse on the client and the maximum number of connections allowed to be pending dispatch on the server.
            //http://stackoverflow.com/questions/3689008/wcf-maxconnections-property

            //Define the FIXED URL that client's will use to communicate with the service.
            // will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath2]
            string serviceEndpointUrl2 = string.Format("{0}{1}", domainNameFixed, servicePath2);

            //Create a UNIQUE URL for the service endpoint given this role instance's dynamic ip address and port:
            //(each load balanced instance will have it's own)
            // will be something like: net.tcp://[111.22.33.444]:[port]/[servicePath2]
            //string serviceListenUrl = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPortDynamic, servicePath2);
            string serviceListenUrl2 = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPort, servicePath2);

            //When creating multiple listneres url must be unique
            testCommunicationListener2.ServiceHost.Description.Endpoints[0].Address = new EndpointAddress(serviceListenUrl2);

            #endregion


            //Create the fixed endpoint for the metadata exchange ("mex") that client's will use:
            //will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath]/mex
            string mexEndpointUrl2 = string.Format("{0}{1}/mex", domainNameFixed, servicePath2);

            //Create the URL that this role instance actually listens on to give it's dynamic ip address & port:
            //will be something like: net.tcp://[111.22.333.44]:[port]/[servicePath2]/mex
            //string mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPortDynamic, servicePath2);
            string mexListenUrl2 = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, servicePath2);



            //Upadted 
            //mexEndpointUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");
            //mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");

            // 3. //Create the mex endpoint, exposing the IMetadataExchange contract over the mex tcp binding, on the Urls we just built:
            // Used to allow MexTcpBinding to allow metadata to be exchanged over tcp
            Binding mexBinding2 = MetadataExchangeBindings.CreateMexTcpBinding();
            mexBinding2.Name = "Binding2"; //<--Mex binding names must be unique to avoid collisions
            // 3. //Create the mex endpoint, exposing the IMetadataExchange contract over the mex tcp binding, on the Urls we just built:
            testCommunicationListener2.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding2, mexEndpointUrl2, new Uri(mexListenUrl2));
            //testCommunicationListener2.ServiceHost.Description.Endpoints[1].Name = "ServicesTestsmex2"; //<-- UNIQUE NAMES MUST BE USED!!!!!!!

            //testCommunicationListener.ServiceHost.Authentication.AuthenticationSchemes = new System.Net.AuthenticationSchemes();


            // 4. Log the creation

            //ServiceEventSource.Current.ServiceMessage(this.Context, serviceName + " Services (From WCF Core Services) are listening on:");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath2 + " (From WCF Core Services) on: " + serviceEndpointUrl2 + "(endpoint)");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath2 + " Services (From WCF Core Services) on: " + serviceListenUrl2 + "(listener)");

            //Remove if MetadataExchangeBinding is not being provided:
            ServiceEventSource.Current.ServiceMessage(this.Context, "--------");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath2 + " Services (From WCF Core Services) on: " + mexEndpointUrl2 + "(mex endpoint)");
            ServiceEventSource.Current.ServiceMessage(this.Context, servicePath2 + " Services (From WCF Core Services) on: " + mexListenUrl2 + "(mex listener)");

            #endregion

            #endregion



            #region Assign Communication Listeners


            return new[] {
                new ServiceInstanceListener((context) => testCommunicationListener1, "Listener1"),
                new ServiceInstanceListener((context) => testCommunicationListener2, "Listener2")          
            };

            #endregion

        }
*/



        // ----- NOT YET USED, PREPPING FOR (DRY) IMPLEMENTATIONS ---------------
        /*
        public class WCFCommunicationListenerFactor
        {
            public ServiceInstanceListener CreateListener(StatelessServiceContext context, string uniqueListenerName, string uniqueMexBindingName, string servicePath, Type serviceContract, Type serviceClass)
            {
                #region create shared endpoint variables/objects

                ////<Endpoint Protocol="tcp" Name="WcfServiceEndpoint" Type="Input" Port="8081" />
                string serviceEndpointName = "WcfServiceEndpoint";     //<-- Same as TCP Endpoint Name from "ServiceManifest.xml":


                //TO DO DETERMINE ROLE ENVIORNMENT:
                //string enviornment = RoleEnvironment.GetConfigurationSettingValue("Environment");
                string enviornment = "local";

                //Get domain name from configuration file (Local, Debug, Stage or Release):
                string domainNameFixed = Settings.ServiceEndpoint.GetEndpoint(enviornment);



                //This is the "Public port" of the "wsfTcpPort" input endpoint configured in ServiceDefinition.csdf
                //It is exposed by the loadbalancer on the CloudService.
                //Value MUST be same as configured in ServiceDefinition.csdf
                //int wcfTcpPortFixed = 8081;

                //Each role instance will have a dynamic ip address, and a dynamic tcp port that the WCF service needs to listen on.
                //Value of endpoint name "WcfTcpPort" MUST be same as the port name configured in ServiceDefinition.csdf
                //string ipAddressDynamic = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WcfTcpPort"].IPEndpoint.Address.ToString();
                //int wcfTcpPortDynamic = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WcfTcpPort"].IPEndpoint.Port;

                //Updated for Service Fabric -----------
                //"Fabric_NodeIPOrFQDN=containerhost1",
                //"Fabric_NodeName=host1",
                //"Fabric_RuntimeConnectionAddress=localhost:19003",
                //Each service instance will have a dynamic ip address, and a dynamic tcp port that the WCF service needs to listen on.
                string ipAddressDynamic = Environment.GetEnvironmentVariable("Fabric_NodeIPOrFQDN");
                int wcfTcpPort = Settings.ServicePorts.GetServicePort(enviornment);
                //string nodeIpAndPortAddress = Environment.GetEnvironmentVariable("Fabric_RuntimeConnectionAddress");

                //Create a TCP binding that will be used to communicate directly over TCP with NO SECURITY required
                NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);

                // We quadruple the MaxReceivedMessageSize of 65536 for large calls like for logs, etc...
                tcpBinding.MaxReceivedMessageSize = 5000000; //<-- Increased to 5mb for Profile Photo Uploads (default is 65536 bytes). This bound on message size is intended to limit exposure to denial of service (DoS) attacks.
                tcpBinding.MaxBufferSize = 5000000; //<-- Increased to 5mb for Profile Photo Uploads (default is 65536 bytes). If you receive more data than you can buffer, the data that exceeds the buffer size remains on the underlying socket until your buffer has room for the rest of the data.
                tcpBinding.MaxBufferPoolSize = 0; //<-- Set to 0 for large messages. Better for Garbage Collection: For larger messages, it’s often best to avoid pooling altogether, which you can accomplish by setting MaxBufferPoolSize=0. You should of course profile your code under different values to determine the settings that will be optimal for your application. 

                

                #endregion

                // 1. Create the Listener Object
                var communicationListener = new WcfCommunicationListener<IContract1>(
                       context,
                       new Service1(),
                       WcfUtility.CreateTcpClientBinding(),
                       serviceEndpointName
                   );

                //    Used toUpdate the ServiceHost object to allow for MetadataExchange Behaviour
                //    Allows WCFTestClient & VS > Add ServiceReference to download service descrition
                //    If removed, clients must have a copy of the Contracts DLL
                ServiceMetadataBehavior metaDataBehavior = new ServiceMetadataBehavior();

                communicationListener.ServiceHost.Description.Behaviors.Add(metaDataBehavior);

                // 3. Add service debug information to include exception details in faults
                communicationListener.ServiceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
                communicationListener.ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });


                #region Endpoint Generation

                //TO DO Inject Contract Type:
                Type contractInterfaceType = typeof(IContract1);
                //Type contractType; <-- Not used



                //Exceptions:
                if (servicePath == "/Services/Platform/Initialization")
                {
                    //For the initialization process we increase the timeout to 5 min.
                    tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(5);
                }

                if (servicePath == "/Services/Application/Images")
                {
                    //For the image processing we increase the timeout to 2.5 min.
                    tcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(2.5);
                }


                //tcpBinding.MaxConnections = 20; //<-- Default is 10: Gets or sets a value that controls the maximum number of connections to be pooled for subsequent reuse on the client and the maximum number of connections allowed to be pending dispatch on the server.
                //http://stackoverflow.com/questions/3689008/wcf-maxconnections-property

                //Define the FIXED URL that client's will use to communicate with the service.
                // will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath]
                string serviceEndpointUrl = string.Format("{0}{1}", domainNameFixed, servicePath);

                //Create a UNIQUE URL for the service endpoint given this role instance's dynamic ip address and port:
                //(each load balanced instance will have it's own)
                // will be something like: net.tcp://[111.22.33.444]:[port]/[servicePath]
                //string serviceListenUrl = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPortDynamic, servicePath);
                string serviceListenUrl = string.Format("net.tcp://{0}:{1}{2}", ipAddressDynamic, wcfTcpPort, servicePath);

                //When creating multiple listneres url must be unique
                communicationListener.ServiceHost.Description.Endpoints[0].Address = new EndpointAddress(serviceListenUrl);

                #endregion


                //Create the fixed endpoint for the metadata exchange ("mex") that client's will use:
                //will be something like: net.tcp://[cloud-service-name.cloudapp.net]:[port]/[servicePath]/mex
                string mexEndpointUrl = string.Format("{0}{1}/mex", domainNameFixed, servicePath);

                //Create the URL that this role instance actually listens on to give it's dynamic ip address & port:
                //will be something like: net.tcp://[111.22.333.44]:[port]/[servicePath]/mex
                //string mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPortDynamic, servicePath);
                string mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, servicePath);


                //Upadted 
                //mexEndpointUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");
                //mexListenUrl = string.Format("net.tcp://{0}:{1}{2}/mex", ipAddressDynamic, wcfTcpPort, "/TestService");

                // 3. //Create the mex endpoint, exposing the IMetadataExchange contract over the mex tcp binding, on the Urls we just built:
                // Used to allow MexTcpBinding to allow metadata to be exchanged over tcp
                Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
                mexBinding.Name = uniqueMexBindingName; //<--Mex binding names must be unique to avoid collisions
                communicationListener.ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, mexEndpointUrl, new Uri(mexListenUrl));

                //testCommunicationListener.ServiceHost.Description.Endpoints[2].Name = "ServicesTestsmex";

                //testCommunicationListener.ServiceHost.Authentication.AuthenticationSchemes = new System.Net.AuthenticationSchemes();


                // 4. Log the creation

                //ServiceEventSource.Current.ServiceMessage(this.Context, serviceName + " Services (From WCF Core Services) are listening on:");
                ServiceEventSource.Current.ServiceMessage(context, servicePath + " (From WCF Core Services) on: " + serviceEndpointUrl + "(endpoint)");
                ServiceEventSource.Current.ServiceMessage(context, servicePath + " Services (From WCF Core Services) on: " + serviceListenUrl + "(listener)");

                //Remove if MetadataExchangeBinding is not being provided:
                ServiceEventSource.Current.ServiceMessage(context, "--------");
                ServiceEventSource.Current.ServiceMessage(context, servicePath + " Services (From WCF Core Services) on: " + mexEndpointUrl + "(mex endpoint)");
                ServiceEventSource.Current.ServiceMessage(context, servicePath + " Services (From WCF Core Services) on: " + mexListenUrl + "(mex listener)");


                //When usng multiple listeners they each MUST have a unique name:
                return new ServiceInstanceListener((context) => communicationListener, uniqueListenerName);
            }
        }

        */


        #endregion
    }
}
