using Sahara.Core.Application.DocumentModels.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.Models
{
    [DataContract]
    public class ProductResults
    {
        [DataMember]
        public List<ProductDocumentModel> Products;

        [DataMember]
        public bool moreResultsExist;

        [DataMember]
        public int page;

        [DataMember]
        public int resultsPerPage;
    }
}
