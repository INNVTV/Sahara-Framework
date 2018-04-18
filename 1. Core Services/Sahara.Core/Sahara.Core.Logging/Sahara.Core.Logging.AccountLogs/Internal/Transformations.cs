using Sahara.Core.Logging.AccountLogs.Models;
using Sahara.Core.Logging.AccountLogs.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.AccountLogs
{
    internal static class Transformations
    {
        public static List<AccountActivityLog> TransformAccountLogTableEntitiesToAccountActivityLogs(IEnumerable<AccountLogTableEntity> tableEntities)
        {
            var activityLogs = new List<AccountActivityLog>();

            if (tableEntities != null)
            {
                foreach(AccountLogTableEntity tableEntity in tableEntities)
                {
                    activityLogs.Add(
                        new AccountActivityLog
                            {
                                Timestamp = tableEntity.Timestamp,
                                Category = tableEntity.Category,
                                Activity = tableEntity.Activity,
                                UserID = tableEntity.UserID,
                                UserName = tableEntity.UserName,
                                UserEmail = tableEntity.UserEmail,
                                ObjectID = tableEntity.ObjectID,
                                ObjectName = tableEntity.ObjectName,
                                Description = tableEntity.Description,
                                Details = tableEntity.Details,
                                IPAddress = tableEntity.IPAddress,
                                Origin = tableEntity.Origin,
                                Object = tableEntity.Object

                            }
                        );
                }
            }

            return activityLogs;
        }
    }
}
