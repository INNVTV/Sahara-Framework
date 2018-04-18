using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.DocumentModels
{
    [DataContract]
    public class AccountSettingsDocumentModel
    {
        [DataMember]
        [JsonProperty(PropertyName = "id")] //<-- Required for all Documents
        public string Id;

        [DataMember]
        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; internal set; }

        //[JsonProperty(PropertyName = "_self")]
        // public string SelfLink { get; internal set; }

        //----------- Isolate Tenant Images via AccountID & DocumentType -------------------------

        //[DataMember]
        //public string AccountID;

        //[DataMember]
        //public string AccountNameKey;

        [DataMember]
        public string DocumentType = "AccountSettings";

        [DataMember]
        public string CustomDomain;

        [DataMember]
        public ContactSettingsModel ContactSettings;

        [DataMember]
        public SalesSettingsModel SalesSettings;

        //[DataMember]
        //public SortSettingsModel SortSettings;

        //------------- Platform Table Data ------------------------

        [DataMember]
        public string Theme; //<-- String tied to available options in platform storage account themes table


    }

    public class ContactSettingsModel
    {
        [DataMember]
        public bool ShowPhoneNumber; //<-- Show/Hide Phone Number on Website
        [DataMember]
        public bool ShowAddress; //<-- Show/Hide Address on Website
        [DataMember]
        public bool ShowEmail; //<-- Show/Hide Email on Website

        [DataMember]
        public ContactInfoModel ContactInfo;
    }

    public class SalesSettingsModel
    {
        [DataMember]
        public bool UseSalesLeads; //<-- Show/Hide Sales Lead Options in Tablet/Website/AccountAdmin

        [DataMember]
        public bool UseSalesAlerts; //<-- Send sales alerts to emails in alert list

        [DataMember]
        public string ButtonCopy; //<-- Copy for the submission buttons that the public will see.

        [DataMember]
        public string DescriptionCopy; //<-- Copy to describe the lead submission process to the public.

        [DataMember]
        public List<string> LeadLabels; //<-- List of leads statuses, labels or "buckets" "new/New" + "archive/Archove" is default (and cannot be deleted)

        [DataMember]
        public List<string> AlertEmails; //<-- Emails alert list

    }

    /*
    public class SortSettingsModel
    {
        [DataMember]
        public SortSettingsItem TruncatedListing { get; set; }

        [DataMember]
        public SortSettingsItem MixedListing { get; set; }

        [DataMember]
        public SortSettingsItem FullListing { get; set; }
    }

    public class SortSettingsItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
    */

    public class ContactInfoModel
    {
        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string Address1 { get; set; }
        [DataMember]
        public string Address2 { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string PostalCode { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}
