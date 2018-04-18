using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Sahara.Core.Common.DocumentModels;
using Sahara.Core.Common.Redis.PlatformManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Platform.Partitioning.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Application.Search;
using Sahara.Core.Settings.Models.Partitions;

namespace Sahara.Core.Platform.Partitioning.Public
{
    public static class StoragePartitioningManager
    {
        #region Get

        public static List<StoragePartition> GetStoragePartitions()
        {
            List<StoragePartition> partitions = new List<StoragePartition>();

            return Sql.Statements.SelectStatements.SelectStoragePartitions();
        }

        #endregion

    }


}