using PlatformAdminSite.PlatformAuthenticationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PlatformAdminSite
{
    /// <summary>
    /// Used when managing user information on the client side after authentication on a web application
    /// </summary>
    [Authorize]
    public static class AuthenticationCookieManager
    {

        public static string authCookieName = "SaharaPlatformAdminAuth";
        public static int expirationHours = 48;

        public static PlatformUser GetAuthenticationCookie()
        {
            PlatformUser platformUser = new PlatformUser();

            //Remove after duplicated to Account Portal:
            //accountUser.Account = new Account();
            //accountUser.Account.PaymentPlan = new PaymentPlan();
            //accountUser.Account.PaymentFrequency = new PaymentFrequency();

            HttpCookie _httpCookie = HttpContext.Current.Request.Cookies.Get(authCookieName);

            //Check if Cookie exists:
            if (_httpCookie != null)
            {

                platformUser.Id = _httpCookie["UserID"];
                platformUser.Email = _httpCookie["Email"];
                platformUser.FirstName = _httpCookie["FirstName"];
                platformUser.LastName = _httpCookie["LastName"];
                platformUser.UserName = _httpCookie["UserName"];
                platformUser.Role = _httpCookie["Role"];
                platformUser.Photo = _httpCookie["Photo"];
                platformUser.Active = Convert.ToBoolean(_httpCookie["Active"]);
                platformUser.CreatedDate = Convert.ToDateTime(_httpCookie["CreatedDate"]);

                //Store for an additional [X] Hours: --------------------
                _httpCookie.Expires = DateTime.Now.AddHours(48);
                //response.Cookies.Add(_httpCookie);
                HttpContext.Current.Response.Cookies.Remove(authCookieName);
                HttpContext.Current.Response.Cookies.Add(_httpCookie);

                #region Remove after duplicated for Account Portal
                //Populate object:
                /*
                accountUser.AccountID = new Guid(_httpCookie["AccountID"].ToString());
                accountUser.AccountName = System.Uri.UnescapeDataString(_httpCookie["AccountName"]);
                accountUser.AccountNameKey = _httpCookie["AccountNameKey"];

                accountUser.Account.AccountID = new Guid(_httpCookie["AccountID"].ToString());
                accountUser.Account.AccountName = System.Uri.UnescapeDataString(_httpCookie["AccountName"]); // HttpUtility.UrlDecode(_httpCookie["AccountName"]);
                accountUser.Account.AccountNameKey = _httpCookie["AccountNameKey"];
                accountUser.Account.DataPartition = _httpCookie["DataPartition"];

                accountUser.Account.StripeCustomerID = _httpCookie["StripeCustomerID"];

                accountUser.Account.Activated = Convert.ToBoolean(_httpCookie["AccountActivated"]);
                accountUser.Account.Active = Convert.ToBoolean(_httpCookie["AccountActive"]);

                accountUser.Account.Locked = Convert.ToBoolean(_httpCookie["AccountLocked"]);
                if (accountUser.Account.Locked)
                {
                    accountUser.Account.LockedDate = Convert.ToDateTime(_httpCookie["AccountLockedDate"]);
                }

                accountUser.Account.PaymentPlanID = Convert.ToInt32(_httpCookie["AccountPaymentPlanID"]);

                accountUser.Account.PaymentPlan.PaymentPlanID = Convert.ToInt32(_httpCookie["AccountPaymentPlanID"]);
                accountUser.Account.PaymentPlan.PaymentPlanName = _httpCookie["AccountPaymentPlanName"];
                accountUser.Account.PaymentPlan.MonthlyRate = Convert.ToDecimal(_httpCookie["AccountPaymentPlanMonthlyRate"]);
                accountUser.Account.PaymentPlan.MaxUsers = Convert.ToInt32(_httpCookie["AccountPaymentPlanMaxUsers"]);
                accountUser.Account.PaymentPlan.MaxObjects = Convert.ToInt32(_httpCookie["AccountPaymentMaxObjects"]);
                accountUser.Account.PaymentPlan.MaxStorage = Convert.ToInt32(_httpCookie["AccountPaymentMaxStorage"]);


                accountUser.Account.Provisioned = Convert.ToBoolean(_httpCookie["AccountProvisioned"]);
                accountUser.Account.Verified = Convert.ToBoolean(_httpCookie["AccountVerified"]);

                try
                {
                    accountUser.Account.VerifiedDate = Convert.ToDateTime(_httpCookie["AccountVerifiedDate"]);
                    accountUser.Account.TrialEndDate = Convert.ToDateTime(_httpCookie["AccountTrialEndDate"]);
                    accountUser.Account.ProvisionedDate = Convert.ToDateTime(_httpCookie["AccountProvisionedDate"]);

                    accountUser.Account.CreatedDate = Convert.ToDateTime(_httpCookie["AccountCreatedDate"]);
                }
                catch
                {

                }

                accountUser.Account.PaymentFrequencyID = Convert.ToInt32(_httpCookie["AccountPaymentFrequencyID"]);

                accountUser.Account.PaymentFrequency.PaymentFrequencyInMonths = Convert.ToInt32(_httpCookie["AccountPaymentFrequencyID"]);



                accountUser.Id = _httpCookie["UserID"];
                accountUser.Email = _httpCookie["Email"];
                accountUser.FirstName = _httpCookie["FirstName"];
                accountUser.LastName = _httpCookie["LastName"];
                accountUser.UserName = _httpCookie["UserName"];

                //Store for an additional [X] Hours: --------------------
                _httpCookie.Expires = DateTime.Now.AddHours(Sahara.Core.EnvironmentSettings.Accounts.Authentication.WebApplicationCookiePersistence);
                //response.Cookies.Add(_httpCookie);
                HttpContext.Current.Response.Cookies.Remove(Sahara.Core.EnvironmentSettings.Accounts.Authentication.WebApplicationCookieName);
                HttpContext.Current.Response.SetCookie(_httpCookie);
                 */
#endregion

            }
            else
            {
                HttpContext.Current.Response.Redirect("/Login/LogOut");   
            }

            //accountUser.Account = AccountManager.UpdateStatus(accountUser.Account);

            return platformUser;

        }

        //Repeated Above for getting users from alternate WCF service client
        public static bool SaveAuthenticationCookie(PlatformManagementService.PlatformUser platformUser)
        {
            DestoryAuthenticationCookie();

            HttpCookie _httpCookie = new HttpCookie(authCookieName);

            //serialize object into Cookie:
            _httpCookie["UserID"] = platformUser.Id;
            _httpCookie["Email"] = platformUser.Email;
            _httpCookie["FirstName"] = platformUser.FirstName;
            _httpCookie["LastName"] = platformUser.LastName;
            _httpCookie["UserName"] = platformUser.UserName;
            _httpCookie["Role"] = platformUser.Role;
            _httpCookie["Photo"] = platformUser.Photo;
            _httpCookie["Active"] = platformUser.Active.ToString();
            _httpCookie["CreatedDate"] = platformUser.CreatedDate.ToString();


            //Expire cookie in [X] Hours -------------------------------
            _httpCookie.Expires = DateTime.Now.AddDays(expirationHours);

            HttpContext.Current.Response.Cookies.Add(_httpCookie);

            return true;
        }

        //Repeated Above for getting users from alternate WCF service client
        public static bool SaveAuthenticationCookie(PlatformAuthenticationService.PlatformUser platformUser)
        {
            DestoryAuthenticationCookie();

            HttpCookie _httpCookie = new HttpCookie(authCookieName);

            //serialize object into Cookie:
            _httpCookie["UserID"] = platformUser.Id;
            _httpCookie["Email"] = platformUser.Email;
            _httpCookie["FirstName"] = platformUser.FirstName;
            _httpCookie["LastName"] = platformUser.LastName;
            _httpCookie["UserName"] = platformUser.UserName;
            _httpCookie["Role"] = platformUser.Role;
            _httpCookie["Photo"] = platformUser.Photo;
            _httpCookie["Active"] = platformUser.Active.ToString();
            _httpCookie["CreatedDate"] = platformUser.CreatedDate.ToString();

            #region REMOVE AFTER PORTED TO ACCOUNT PORTAL
            //serialize object into Cookie:
            /*
            _httpCookie["AccountID"] = accountUser.Account.AccountID.ToString();
            _httpCookie["AccountName"] = System.Uri.EscapeDataString(accountUser.Account.AccountName); //HttpUtility.HtmlEncode(accountUser.Account.AccountName);

            _httpCookie["AccountNameKey"] = accountUser.Account.AccountNameKey;
            _httpCookie["DataPartition"] = accountUser.Account.DataPartition;

            _httpCookie["StripeCustomerID"] = accountUser.Account.StripeCustomerID;

            _httpCookie["UserID"] = accountUser.Id;
            _httpCookie["Email"] = accountUser.Email;
            _httpCookie["FirstName"] = accountUser.FirstName;
            _httpCookie["LastName"] = accountUser.LastName;
            _httpCookie["UserName"] = accountUser.UserName;

            _httpCookie["AccountActivated"] = accountUser.Account.Activated.ToString();
            _httpCookie["AccountActive"] = accountUser.Account.Active.ToString();
            

            _httpCookie["AccountProvisioned"] = accountUser.Account.Provisioned.ToString();
            _httpCookie["AccountVerified"] = accountUser.Account.Verified.ToString();

            _httpCookie["AccountVerifiedDate"] = accountUser.Account.VerifiedDate.ToString();
            _httpCookie["AccountTrialEndDate"] = accountUser.Account.TrialEndDate.ToString();
            _httpCookie["AccountProvisionedDate"] = accountUser.Account.ProvisionedDate.ToString();

            _httpCookie["AccountCreatedDate"] = accountUser.Account.CreatedDate.ToString();

            _httpCookie["AccountPaymentFrequencyID"] = accountUser.Account.PaymentFrequencyID.ToString();


            _httpCookie["AccountLocked"] = accountUser.Account.Locked.ToString();
            _httpCookie["AccountLockedDate"] = accountUser.Account.LockedDate.ToString();
            _httpCookie["AccountPaymentPlanID"] = accountUser.Account.PaymentPlanID.ToString();
            _httpCookie["AccountPaymentPlanName"] = accountUser.Account.PaymentPlan.PaymentPlanName.ToString();
            _httpCookie["AccountPaymentPlanMonthlyRate"] = accountUser.Account.PaymentPlan.MonthlyRate.ToString();
            _httpCookie["AccountPaymentPlanMaxUsers"] = accountUser.Account.PaymentPlan.MaxUsers.ToString();
            _httpCookie["AccountPaymentMaxObjects"] = accountUser.Account.PaymentPlan.MaxObjects.ToString();
            _httpCookie["AccountPaymentMaxStorage"] = accountUser.Account.PaymentPlan.MaxStorage.ToString();

             * */
            #endregion

            //Expire cookie in [X] Hours -------------------------------
            _httpCookie.Expires = DateTime.Now.AddDays(expirationHours);

            HttpContext.Current.Response.Cookies.Add(_httpCookie);

            return true;
        }

        public static bool DestoryAuthenticationCookie()
        {
            //Expire & clear the cookie
            HttpCookie _currentCookie = HttpContext.Current.Request.Cookies[authCookieName];
            if (_currentCookie != null)
            {
                HttpContext.Current.Response.Cookies.Remove(authCookieName);
                _currentCookie.Expires = DateTime.Now.AddDays(-10);
                _currentCookie.Value = null;
                HttpContext.Current.Response.Cookies.Add(_currentCookie); 
            }

            return true;
        }

    }
}