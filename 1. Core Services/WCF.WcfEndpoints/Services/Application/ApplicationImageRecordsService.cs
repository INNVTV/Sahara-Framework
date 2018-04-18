using Sahara.Core.Accounts.Commerce.Public;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;
using WCF.WcfEndpoints.Contracts.Application;
using Sahara.Core.Imaging.Models;
using System.ServiceModel;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Drawing;
//using Sahara.Core.Application.ApplicationImages;
//using Sahara.Core.Application.ApplicationImages.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Images.Records.Models;
using Sahara.Core.Application.Images.Records;
using Sahara.Core.Application.Images.Records.TableEntities;

namespace WCF.WcfEndpoints.Service.Application
{
    
    public class ApplicationImageRecordsService : IApplicationImageRecordsService
    {

        /// <summary>
        /// Used by Clients to get reference to the ImageRecordGroup/ImageRecord Models
        /// (Clients should merge records with formats locally to avoid WCF call)
        /// </summary>
        /// <param name="accountNameKey"></param>
        /// <param name="imageFormatGroupTypeNameKey"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public List<ImageRecordGroupModel> GetImageRecordsForObject(string accountNameKey, string imageFormatGroupTypeNameKey, string objectId)
        {
            var account = AccountManager.GetAccount(accountNameKey);

            return ImageRecordsManager.GetImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, accountNameKey, imageFormatGroupTypeNameKey, objectId);
        }

        //Only use to expose TableEntity object to clients
        public ImageRecordTableEntity GetEmptyImageRecordTableEntityReference()
        {
            return new ImageRecordTableEntity();
        }
    }
}
