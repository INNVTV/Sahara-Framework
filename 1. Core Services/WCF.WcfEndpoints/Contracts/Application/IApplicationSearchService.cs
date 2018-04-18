using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Application.Products.Models;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Application
{
    [ServiceContract]
    public interface IApplicationSearchService
    {
        // ------------ INDEXES -----------

        /// <summary>
        /// Senda a message queue to the worker role to reindex all products, fields, etc...
        /// 
        /// Used For: 1. If a property is deleted (requiring an index refresh to recreate fields since fields cannot be deleted manually)
        /// Used For: 2. If any searhc issues occur occur so that all product documents can be reset
        /// Used For: 3. If an account (or all accounts) need to migrate to a new search service plan. This isused on each account after the migration. You can move each plan one-by one and keep BOTH plans up until after migration is complete.
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        DataAccessResponseType ReindexProducts(string accountNameKey, string sharedClientKey);

        // ------------ FACETS -----------

        //[OperationContract]
        //List<ProductSearchFacetType> GetProductFacetTypes();

        //[OperationContract]
        //List<ProductSearchFacetStyle> GetProductFacetStyles();

        [OperationContract]
        List<ProductSearchFacet> GetProductFacets(string accountNameKey, bool includeHidden, string sharedClientKey);

        //[OperationContract]
        //DataAccessResponseType CreateProductFacet(string account, string propertyNameKey, string facetType, string facetStyle);


        // ------------ SORTABLES -----------


        [OperationContract]
        List<ProductSearchSortable> GetProductSortables(string accountNameKey, string sharedClientKey);

    }
}
