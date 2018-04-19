# The Sahara Framework
A generalized platform for creating and managing multi-tenant SaaS applications on the Azure cloud. 

# Key Features

  * Customizable on-boarding process for new tenants.
  * Simple tenant and resource management.
  * Smart partitioning of resources across tenants.
  * Easily develop web and native applications.
  * Easily scale up or down as your needs evolve.
  * Public and private API endpoints for the platform and each tenant.
  * Build any kind of business logic required.
  * Integration with Stripe for billing/dunning.
  * Integration with CloudFlare for Domain & Subdomain Management, SSL, DDoS protection, and more. 
  * Built on a microservices architecture with Service Fabric.
  * Focus on your application - not the plumbing!

# Key Projects:

Each root folder (1-8) corresponds to an independent Solution that can be managed by separate teams or an individual. These solutions/projects are designed to be managed within Visual Studio Online (Team Services) and utilize the VSO Build and Release Management systems for a controlled devops flow to the associated resources and environments (Test/Stage/Production) on the Azure Cloud.

## Core Services
The Service Fabric components that are the heartbeat that the entire platform runs upon. This includes:

  * **Sahara.Core**
    * Core business logic
    * Most development will occur within [these classes] (https://github.com/INNVTV/Sahara-Framework/tree/master/1.%20Core%20Services/Sahara.Core)
  * **Custodian**
    * Scheduled tasks
    * Garbage collection
  * **Worker**
    * Message queue processing
    * Background tasks 
  * **Webhooks**
    * Stripe integration
    * Remote exception logging
    * 3rd party service integrations
  * **WCFEndpoints**
    * Private communications fabric
    * Component to Core Services Messaging  
  * **RegistrationApi**
    * Centralized authority for remote account registrations 

## Platform Admin
Dashboard for managing platform resources, accounts and tenant onboarding. Includes:

  * Partition Management
  * Tenant Management
  * Exception Logs
  * Billing Reports
  * Dunning Attempts/Alerts

## Account Admin
Dashboard for tenants to manage their accounts:

  * Account Management
  * Billing Management
  * Content Management
  * Search Engine
  * API Management
  * API Key Management


## Account Public API
Public API endpoints for tenants to access (or share) their data.

## Account Website
Public Website that allows tenants to share their data to the public. Includes a robust search engine.

## Account Registration Service
API Endpoints for registering new tenants. Allows for both web and native applications to consume.

## Account Registration Site
Website that uses the registration endpoint for signing up new tenants.

## Imaging Service
A service that allows for image processing to be handled as a remote background task. Accessed through a set of APIs - orchestrates through Blob Storage outside of Core Services.


# Installation

Each solution should have an isolated project with controlled access set up in Visual Studio Online (Team Services). Build and Release jobs can then be configured for each environment you will be targeting. You will want to move all of your access keys to Azure Key Vault and have your build system inject them into your project during compilation to maintain ideal security.


# Configuration

**Note:** All configurable variables can be found by searching for: **"[Config_"**

## CloudFlare
Found within the **Sahara.Core.Settings.Services.CloudFlare** class.

Used to configure or remove subdomains for tenants during provisioning and deprovisioning.

Please reference the CloudFlare API documentation for further information.

## Stripe
Found within the **Sahara.Core.Settings.Services.Stripe** class.

## Google Maps
Found within the **Sahara.Core.Settings.Services.GoogleMaps** class.

## Send Grid
Found within the **Sahara.Core.Settings.Services.SendGrid** class.

## Application Name and Default Time Zone
Found within the **Sahara.Core.Settings.Application** class.

## Reserved Account Names
Found within the **Sahara.Core.Settings.Accounts.Registration** class .re

## Roles
Found within the **Sahara.Core.Settings.Accounts.Users.Authorization.Roles** class.

## Azure Resources
Found within the **Sahara.Core.Settings.Azure.** classes 

---

**Note:** All of the above configurations *should* be moved to **Azure Key Vault** to secure your production resources!

---

