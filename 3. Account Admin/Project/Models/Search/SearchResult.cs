using AccountAdminSite.ApplicationImageRecordsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.Search
{
    public class SearchResults
    {
        public long? Count  { get; set; }
        public int Returned { get; set; }
        public string Range { get; set; }
        public int Remaining { get; set; }

        public double? Coverage { get; set; }
        public Microsoft.Azure.Search.Models.FacetResults Facets { get; set; }
        public Microsoft.Azure.Search.Models.SearchContinuationToken ContinuationToken { get; set; }

        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public double Score { get; set; }
        public Microsoft.Azure.Search.Models.Document Document { get; set; }
        public IDictionary<string, object> Images { get; set; }
    }
}