using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace Account.Public.Website
{
    public static class WCFManager
    {
        public static string UserFriendlyExceptionMessage = "An exception occurred while attempting to communicate with Core Services. Please try again later.";

        #region Close/Abort Connections & Manage WCF Exceptions

        public static void CloseConnection(ICommunicationObject serviceClient, string exceptionMessage = null, string currentMethod = null)
        {
            if (exceptionMessage == null && currentMethod == null)
            {
                //Attempt to close the WCF connection normally:
                #region Close the connection & handle any potential exceptions

                try
                {
                    serviceClient.Close();
                    //TODO: Log: error message for WCF connection closure
                }
                catch (CommunicationException e)
                {
                    var error = e.Message;
                    serviceClient.Abort();
                    //TODO: Log: error message for WCF connection closure
                }
                catch (TimeoutException e)
                {
                    var error = e.Message;
                    serviceClient.Abort();
                    //TODO: Log: error message for WCF connection closure
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    serviceClient.Abort();
                    //TODO: Log: error message for WCF connection closure
                }

                #endregion
            }
            else
            {
                //An exception occured while using the WCF connection, abort and handle the exception:
                #region Abort connection and handle the exception

                serviceClient.Abort();


                string error = exceptionMessage.ToString();
                string method = String.Empty;
                try
                {
                    method = currentMethod.ToString();
                }
                catch
                {

                }


                //TODO: Log: error message for WCF exception with error and method

                #endregion
            }
        }

        #endregion
    }
}