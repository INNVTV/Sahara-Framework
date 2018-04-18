using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformAdminSite.Models.Infrastructure
{
    public class DocumentTierModel
    {
        public string DocumentTierId { get; set; }
        public List<DocumentCollectionModel> DocumentCollections { get; set; }
    }
}
