using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountBillingService
    {

        /*==================================================================================
        * Payments (Charges)
        ==================================================================================*/

        [OperationContract]
        Charge GetPayment(string chargeId, string sharedClientKey);

        [OperationContract]
        List<Charge> GetPaymentHistory(int itemLimit, string accountId, string sharedClientKey);

        [OperationContract]
        List<Charge> GetPaymentHistory_Next(int itemLimit, string startingAfterChargeeId, string accountId, string sharedClientKey);

        [OperationContract]
        List<Charge> GetPaymentHistory_Last(int itemLimit, string endingBeforeChargeId, string accountId, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RefundPayment(string accountId, string chargeId, decimal refundAmount, string requesterID, RequesterType requesterType, string sharedClientKey);


     /*==================================================================================
      * Invoices 
      ==================================================================================*/

        [OperationContract]
        Invoice GetInvoice(string invoiceId, string sharedClientKey);

        [OperationContract]
        Invoice GetUpcomingInvoice(string accountId, string sharedClientKey);


        [OperationContract (Name="GetInvoiceHistory")]
        List<Invoice> GetInvoiceHistory(int itemLimit, string accountId, string sharedClientKey);
        [OperationContract(Name = "GetInvoiceHistory_ByDateRange")]
        List<Invoice> GetInvoiceHistory(int itemLimit, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey);


        [OperationContract]
        List<Invoice> GetInvoiceHistory_Next(int itemLimit, string startingAfterInvoiceId, string accountId, string sharedClientKey);
        [OperationContract]
        List<Invoice> GetInvoiceHistory_Last(int itemLimit, string endingBeforeInvoiceId, string accountId, string sharedClientKey);


        [OperationContract]
        List<Invoice> GetInvoiceHistory_ByDateRange_Next(int itemLimit, string startingAfterInvoiceId, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey);
        [OperationContract]
        List<Invoice> GetInvoiceHistory_ByDateRange_Last(int itemLimit, string endingBeforeInvoiceId, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey);


      /*==================================================================================
      * Dunning 
      ==================================================================================*/

        [OperationContract]
        int GetDunningAttemptsCount(string accountId, string sharedClientKey);

        [OperationContract]
        List<DunningAttempt> GetDunningAttempts(string accountId, string sharedClientKey);


    }


}
