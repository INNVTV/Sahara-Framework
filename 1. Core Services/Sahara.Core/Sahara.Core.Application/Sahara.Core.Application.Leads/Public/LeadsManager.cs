using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.Settings;
using Sahara.Core.Application.Leads.Models;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Leads
{
    public static class LeadsManager
    {
        #region Settings (Labels)

        /// <summary>
        /// Create a new label for managing sales leads. (limit 15)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="labelName"></param>
        /// <returns></returns>
        public static DataAccessResponseType CreateLabel(Account account, string labelName)
        {
            #region Validate Sales Lead Label Requirements

            if(labelName.Length > 15)
            {
                //Table names cannot exceed 63 characters.
                //+ 3 for "acc"
                //+ 32 for [accountID]
                //+ 5 for "leads"
                //23 remaining (we do 15 for readability & safety)
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Labels cannot be longer than 15 characters" };
            }
            if (labelName.Contains("  "))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Labels cannot contain any double spaces" };
            }
            
            if(Regex.IsMatch(labelName.Replace("-", "").Replace(" ", ""), "^[a-zA-Z0-9]*$") == false)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Labels cannot contain any special characters" };
            }

            #endregion

            var accountSettings = AccountSettingsManager.GetAccountSettings(account);

            if(accountSettings.SalesSettings.LeadLabels.Count >= 15)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot have more than 15 labels for sales leads" };
            }

            #region validate name does not already exist

            foreach(string existingLabel in accountSettings.SalesSettings.LeadLabels)
            {
                if(existingLabel.ToLower().Replace("-", "").Replace(" ", "") == labelName.ToLower().Replace("-", "").Replace(" ", ""))
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This label already exists, or is a reserved label name" };
                }
            }

            #endregion

            accountSettings.SalesSettings.LeadLabels.Add(labelName);

            return AccountSettingsManager.UpdateAccountSettings(account, accountSettings);
        }

        public static DataAccessResponseType RemoveLabel(Account account, string labelName)
        {

            var accountSettings = AccountSettingsManager.GetAccountSettings(account);

            #region validate

            if(labelName.ToLower() == "new" || labelName.ToLower() == "deleted")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot delete a reserved label" };
            }

            #endregion

            bool labelFound = false;

            foreach (string existingLabel in accountSettings.SalesSettings.LeadLabels)
            {
                if (existingLabel.ToLower() == labelName.ToLower())
                {
                    labelFound = true;
                }
            }

            if(labelFound)
            {
                accountSettings.SalesSettings.LeadLabels.RemoveAt(accountSettings.SalesSettings.LeadLabels.IndexOf(labelName));
                return AccountSettingsManager.UpdateAccountSettings(account, accountSettings);
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Could not find label to remove" };
            }
            
        }

        #endregion


        #region (Legacy)

        /* Account Admin now deals with SALES LEADS DIRECTLY for performance

        #region Get Sales Leads

        public static List<SalesLead> GetSalesLeads(string accountId, string label, int skip, int take)
        {
            return Transformations.TransformSalesLeadTableEntitiesToSalesLead(Internal.GetSalesLeads(accountId, label, skip, take));
        }

        #endregion

        #region Update Sales Lead

        //public static DataAccessResponseType UpdateSalesLead(string accountId, string partitionKey, string rowKey, SalesLead salesLead)
        //{
            //return Internal.UpdateSalesLead(accountId, Transformations.TransformSalesLeadToSalesLeadTableEntity(salesLead));
        //}

        #endregion

        #region Move Sales Lead

        public static DataAccessResponseType MoveSalesLead(string accountId, string partitionKey, string rowKey, string labelFrom, string labelTo)
        {
            return Internal.MoveSalesLead(accountId, partitionKey, rowKey, labelFrom, labelTo);
        }
        

        #endregion

*/

        #endregion

    }
}
