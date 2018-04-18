using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Models;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Reports.Models;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Snapshots.Models;
using Sahara.Core.Platform.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Platform
{
    [ServiceContract]
    public interface IPlatformBillingService
    {

        /*==================================================================================
        * Balance Transactions 
        ==================================================================================*/

        [OperationContract]
        SourceBalanceTransactions GetBalanceTransactionsForSource(string sourceId, string sharedClientKey);


        /*==================================================================================
        * Transfers 
        ==================================================================================*/

        [OperationContract]
        Transfer GetTransfer(string invoiceId, string sharedClientKey);

        [OperationContract(Name = "GetTransferHistory")]
        List<Transfer> GetTransferHistory(int itemLimit, string sharedClientKey);
        [OperationContract(Name = "GetTransferHistory_ByDateRange")]
        List<Transfer> GetTransferHistory(int itemLimit, DateTime startDate, DateTime endDate, string sharedClientKey);

        [OperationContract]
        List<Transfer> GetTransferHistory_Next(int itemLimit, string startingAfterTransferId, string sharedClientKey);
        [OperationContract]
        List<Transfer> GetTransferHistory_Last(int itemLimit, string endingBeforeTransferId, string sharedClientKey);

        [OperationContract]
        List<Transfer> GetTransferHistory_ByDateRange_Next(int itemLimit, string startingAfterTransferId, DateTime startDate, DateTime endDate, string sharedClientKey);
        [OperationContract]
        List<Transfer> GetTransferHistory_ByDateRange_Last(int itemLimit, string endingBeforeTransferId, DateTime startDate, DateTime endDate, string sharedClientKey);


    }





}
