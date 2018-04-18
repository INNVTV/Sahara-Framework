using Sahara.Core.Accounts;
using Sahara.Core.Application.Search;
using Sahara.Core.Application.Search.Facets.Public;
using Sahara.Core.Application.Search.Sorting.Public;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.ResponseTypes;
using WCF.WcfEndpoints.Contracts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Service.Application
{
    public class ApplicationSearchService : IApplicationSearchService
    {
        #region Product Indexes

        public DataAccessResponseType ReindexProducts(string accountNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return new DataAccessResponseType();
        }

        #endregion

        #region Product Facets

        //public List<ProductSearchFacetType> GetProductFacetTypes()
        //{
            //return new List<ProductSearchFacetType>();
        //}

        //public List<ProductSearchFacetStyle> GetProductFacetStyles()
        //{
            //return new List<ProductSearchFacetStyle>();
        //}

        public List<ProductSearchFacet> GetProductFacets(string accountNameKey, bool includeHidden, string sharedClientKey)//, bool includeHidden = true)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return ProductFacetManager.GetProductFacets(accountNameKey, includeHidden); //<-- Had to make own class to avoid circular reference with .Search Class
        }

        //public DataAccessResponseType CreateProductFacet(string account, string propertyNameKey, string facetType, string facetStyle)
        //{
        //return new DataAccessResponseType();
        //}

        #endregion

        #region Product Sortables

        public List<ProductSearchSortable> GetProductSortables(string accountNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return ProductSortingManager.GetProductSortables(accountNameKey); //<-- Had to make own class to avoid circular reference with .Search Class
        }

        #endregion
    }
}
