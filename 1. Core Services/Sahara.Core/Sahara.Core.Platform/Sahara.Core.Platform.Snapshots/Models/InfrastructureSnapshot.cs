using Sahara.Core.Logging.PlatformLogs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Snapshots.Models
{
    [DataContract]
    public class InfrastructureSnapshot
    {
        [DataMember]
        public List<PlatformActivityLog> Errors_Log;
        [DataMember]
        public bool Errors_Last24Hours;
        [DataMember]
        public bool Errors_Last3Days;
        [DataMember]
        public bool Errors_Last7Days;
        [DataMember]
        public bool Errors_Last30Days;


        [DataMember]
        public CustodianSnapshot Custodian;
        [DataMember]
        public WorkerSnapshot Worker;
    }


    [DataContract]
    public class CustodianSnapshot
    {
        [DataMember]
        public DateTime LastRun;

        [DataMember]
        public DateTime NextRun;

        [DataMember]
        public int FrequencyMilliseconds;

        [DataMember]
        public string FrequencyDescription;

        [DataMember]
        public bool IsSleeping;

        [DataMember]
        public bool IsRunning;
    }

    [DataContract]
    public class WorkerSnapshot
    {
        [DataMember]
        public DateTime LastRun;

        [DataMember]
        public DateTime NextRun;

        [DataMember]
        public int FrequencyMilliseconds;

        [DataMember]
        public string FrequencyDescription;

        [DataMember]
        public bool IsSleeping;

        [DataMember]
        public bool IsRunning;
    }
}
