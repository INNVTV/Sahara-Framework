using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.DocumentModels
{
    /// <summary>
    /// Used to store info about the collection
    /// Updated via trigger for every collection transaction
    /// </summary>
    public class CollectionPropertiesDocumentModel
    {

        [JsonProperty(PropertyName = "id")] //<-- Required for all Documents
        public string Id;

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; internal set; }

        //------------------------------------

        public string DocumentType = "CollectionProperties"; //<-- Used to differentiate Properties Document so Deletes and Migration COunts are affected by TestDOcuments only

        public int ProductCount; //<-- Only updated by incrments/decrements for ApplicationImages for ALL accounts

        // To reflect accuratly must be incremented to include itself (+1) as well as every AccountProperties document ( + (partition.tenantcount * 1))
        public int DocumentCount; //<-- Only updated by incrments/decrements for ALL document types for ALL accounts


    }
}
