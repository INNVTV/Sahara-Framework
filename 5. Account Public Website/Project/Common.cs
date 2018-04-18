using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Account.Public.Website
{
    public static class Common
    {
        public static string SharedClientKey = "";

        public static string GetSubDomain(Uri url)
        {
            try
            {
                string host = url.Host;

                if(host.Contains(CoreServices.PlatformSettings.Urls.AccountSiteDomain))//<--using subdomains on our default website
                {
                    if (host.Split('.').Length > 2) 
                    {
                        int firstIndex = host.IndexOf(".");
                        string subdomain = host.Substring(0, firstIndex);

                        if (subdomain != "www" && !String.IsNullOrEmpty(subdomain))
                        {
                            return subdomain;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                else if(host.Split('.').Length > 1) //<--might be using custom domain / run check
                {                

                    foreach(var customDomain in CoreServices.PlatformSettings.CustomDomains)
                    {
                        if(customDomain.Domain == host.ToLower().Replace("www.", ""))
                        {
                            return customDomain.AccountNameKey;
                        }
                    }
                }
                

                //no strings returned, check if this is a local version of the site in development
                if (EnvironmentSettings.CurrentEnvironment.Site == "local")
                {
                    return ConfigurationManager.AppSettings["LocalDebugAccount"];
                }
                
            }
            catch
            {

            }

            return string.Empty;
        }
    }
}