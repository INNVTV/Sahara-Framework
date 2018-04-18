using AccountAdminSite.AccountManagementService;
using System;
using System.Web;
using System.Web.Mvc;


namespace AccountAdminSite
{
    /// <summary>
    /// Used when managing user information on the client side after authentication on a web application
    /// </summary>
    [Authorize]
    public static class AuthenticationCookieManager
    {
        
        public static string authCookieName = "SaharaAccountAdminAuth";
        public static int expirationHours = 48;

        public static AccountUser GetAuthenticationCookie()
        {
            AccountUser accountUser = new AccountUser();

            HttpCookie _httpCookie = HttpContext.Current.Request.Cookies.Get(authCookieName);

            //Check if Cookie exists:
            if (_httpCookie != null)
            {

                accountUser.Id = _httpCookie["UserID"];
                accountUser.Email = _httpCookie["Email"];
                accountUser.FirstName = _httpCookie["FirstName"];
                accountUser.LastName = _httpCookie["LastName"];
                accountUser.UserName = _httpCookie["UserName"];
                accountUser.Photo = _httpCookie["Photo"];
                accountUser.Role = _httpCookie["Role"];
                accountUser.Active = Convert.ToBoolean(_httpCookie["Active"]);
                accountUser.CreatedDate = Convert.ToDateTime(_httpCookie["CreatedDate"]);
                accountUser.AccountOwner = Convert.ToBoolean(_httpCookie["Owner"]);

                accountUser.AccountID = new Guid(_httpCookie["AccountID"]);
                accountUser.AccountName = _httpCookie["AccountName"];
                accountUser.AccountNameKey = _httpCookie["AccountNameKey"];

                //Store for an additional [X] Hours: --------------------
                _httpCookie.Expires = DateTime.Now.AddHours(48);
                //response.Cookies.Add(_httpCookie);
                HttpContext.Current.Response.Cookies.Remove(authCookieName);
                HttpContext.Current.Response.Cookies.Add(_httpCookie);


            }
            else
            {
                HttpContext.Current.Response.Redirect("/Login/LogOut");   
            }

            //accountUser.Account = AccountManager.UpdateStatus(accountUser.Account);

            return accountUser;

        }

        //Repeated Above for getting users from alternate WCF service client
        public static bool SaveAuthenticationCookie(AccountAuthenticationService.AccountUser accountUser)
        {
            DestoryAuthenticationCookie();

            HttpCookie _httpCookie = new HttpCookie(authCookieName);



            //serialize object into Cookie:
            _httpCookie["UserID"] = accountUser.Id;
            _httpCookie["Email"] = accountUser.Email;
            _httpCookie["FirstName"] = accountUser.FirstName;
            _httpCookie["LastName"] = accountUser.LastName;
            _httpCookie["UserName"] = accountUser.UserName;
            _httpCookie["Role"] = accountUser.Role;
            _httpCookie["Active"] = accountUser.Active.ToString();
            _httpCookie["CreatedDate"] = accountUser.CreatedDate.ToString();
            _httpCookie["Owner"] = accountUser.AccountOwner.ToString();

            _httpCookie["Photo"] = accountUser.Photo;
            _httpCookie["AccountID"] = accountUser.AccountID.ToString();
            _httpCookie["AccountName"] = accountUser.AccountName.ToString();
            _httpCookie["AccountNameKey"] = accountUser.AccountNameKey.ToString();


            //Expire cookie in [X] Hours -------------------------------
            _httpCookie.Expires = DateTime.Now.AddDays(expirationHours);

            HttpContext.Current.Response.Cookies.Add(_httpCookie);

            return true;
        }

        //Repeated Above for getting users from alternate WCF service client
        public static bool SaveAuthenticationCookie(AccountManagementService.AccountUser accountUser)
        {
            DestoryAuthenticationCookie();

            HttpCookie _httpCookie = new HttpCookie(authCookieName);

            //serialize object into Cookie:
            _httpCookie["UserID"] = accountUser.Id;
            _httpCookie["Email"] = accountUser.Email;
            _httpCookie["FirstName"] = accountUser.FirstName;
            _httpCookie["LastName"] = accountUser.LastName;
            _httpCookie["UserName"] = accountUser.UserName;
            _httpCookie["Role"] = accountUser.Role;
            _httpCookie["Active"] = accountUser.Active.ToString();
            _httpCookie["CreatedDate"] = accountUser.CreatedDate.ToString();
            _httpCookie["Owner"] = accountUser.AccountOwner.ToString();

            _httpCookie["Photo"] = accountUser.Photo;
            _httpCookie["AccountID"] = accountUser.AccountID.ToString();
            _httpCookie["AccountName"] = accountUser.AccountName.ToString();
            _httpCookie["AccountNameKey"] = accountUser.AccountNameKey.ToString();


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