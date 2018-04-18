using Sahara.Core.Accounts.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountCommerceService
    {
        #region Card Management

        /*==================================================================================
        * CARD MANAGEMENT
        ==================================================================================

        [OperationContract]
        AccountCreditCardInfo GetCreditCardInfo(string accountID, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType AddCreditCard(string accountID, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdateCreditCard(string accountID, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType);
*/
        #endregion

        #region Subscriptions

        /*==================================================================================
        * SUBSCRIPTIONS
        ==================================================================================

        //[OperationContract]
        //DataAccessResponseType CreateSubscripton(string accountID, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType CreateSubscripton(string accountID, string planName, string frequencyMonths, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdateAccountPlan(string accountID, string planName, string frequencyMonths, string requesterId, RequesterType requesterType);
*/
        #endregion

        #region Credits

        /*==================================================================================
        * CREDITS
        ==================================================================================*/

        [OperationContract]
        int GetDollarsToCreditsExchangeRate(decimal dollarAmoun, string sharedClientKeyt);

        [OperationContract]
        int GetCredits(string accountID, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType BuyCredits(string accountID, decimal dollarAmount, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType SpendCredits(string accountID, int creditAmount, string description, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType TradeCredits(string fromAccountID, string toAccountID, int creditAmount, string description, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);


        #endregion

        #region Services

        /*==================================================================================
        * SERVICES
        ==================================================================================

        [OperationContract]
        int GetServices();

        [OperationContract]
        DataAccessResponseType BuyService(string accountID, int serviceId, string requesterId, RequesterType requesterType);
*/
        #endregion

        #region Products

        /*==================================================================================
        * PRODUCTS
        ==================================================================================

        [OperationContract]
        int GetProducts();

        [OperationContract]
        DataAccessResponseType BuyProduct(string accountID, int productId, int quantity, string requesterId, RequesterType requesterType);
*/
        #endregion
    }
}
