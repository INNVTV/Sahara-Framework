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

    }
}
