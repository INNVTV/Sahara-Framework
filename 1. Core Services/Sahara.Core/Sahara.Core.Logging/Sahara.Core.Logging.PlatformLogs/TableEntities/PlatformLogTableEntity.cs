using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.PlatformLogs.TableEntities
{
    /// <summary>
    /// A generic representation of the various internal log entities for public consumption. used as a return type for log retreival methods 
    /// Used only for log retreival, NOT for log creation
    /// </summary>
    public class PlatformLogTableEntity : TableEntity
    {
        public PlatformLogTableEntity()
        {

        }

        public string Category { get; set; }
        public string Activity { get; set; }

        public string Description { get; set; }
        public string Details { get; set; }

        public string UserID { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }

        public string IPAddress { get; set; } 
        public string Origin { get; set; }

        public string AccountID { get; set; }
        public string AccountName { get; set; }

        public string Object { get; set; }

    }
}
