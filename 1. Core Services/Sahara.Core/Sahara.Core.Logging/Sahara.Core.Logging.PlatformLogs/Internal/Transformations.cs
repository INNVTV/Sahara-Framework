using Sahara.Core.Logging.PlatformLogs.Models;
using Sahara.Core.Logging.PlatformLogs.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.PlatformLogs
{
    internal static class Transformations
    {
        public static List<PlatformActivityLog> TransformPlatformLogTableEntitiesToPlatformActivityLogs(IEnumerable<PlatformLogTableEntity> tableEntities)
        {
            var activityLogs = new List<PlatformActivityLog>();

            if (tableEntities != null)
            {
                foreach(PlatformLogTableEntity tableEntity in tableEntities)
                {
                    activityLogs.Add(
                        new PlatformActivityLog
                            {
                                Timestamp = tableEntity.Timestamp,
                                Category = tableEntity.Category,
                                Activity = tableEntity.Activity,
                                UserID = tableEntity.UserID,
                                UserName = tableEntity.UserName,
                                UserEmail = tableEntity.UserEmail,
                                Description = tableEntity.Description,
                                Details = tableEntity.Details,
                                IPAddress = tableEntity.IPAddress,
                                Origin = tableEntity.Origin,
                                AccountID = tableEntity.AccountID,
                                AccountName = tableEntity.AccountName,
                                Object = tableEntity.Object


                            }
                        );
                }
            }

            return activityLogs;
        }
    }
}
