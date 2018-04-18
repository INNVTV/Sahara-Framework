using InventoryHawk.Account.Public.Api.ApplicationSearchService;
using InventoryHawk.Account.Public.Api.Models.Json.Search;
using InventoryHawk.Account.Public.Api.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Transforms.Json
{
    public static class SearchTransforms
    {
        public static SearchResultsJson ProductResults(SearchResults searchResults)
        {
            //Transform categories into JSON and cache locally
            var searchResultsJson = new SearchResultsJson
            {
                count = searchResults.Count,
                returned = searchResults.Returned,
                range = searchResults.Range,
                remaining = searchResults.Remaining
            };


            searchResultsJson.items = Dynamics.Products.TransformDynamicProductsListForJson(searchResults.Results);
            

            return searchResultsJson;
        }

        public static List<ProductSearchFacetJson> ProductFacets(List<ProductSearchFacet> facetsIn)
        {
             var facetsOut = new List<ProductSearchFacetJson>();

            foreach (ProductSearchFacet facetModel in facetsIn)
            {
                var facetJson = new ProductSearchFacetJson
                {
                    name = facetModel.PropertyName,
                    key = facetModel.SearchFieldName,
                    type = facetModel.PropertyType,
                };

                facetJson.values = new List<ProductSearchFacetJsonValue>();

                if(facetModel.Values != null)
                {
                    foreach (FacetValue value in facetModel.Values)
                    {
                        facetJson.values.Add(new ProductSearchFacetJsonValue
                        {
                            name = value.Name,
                            filter = value.Filter,
                            count = value.Count,
                            image = value.Image
                            //value = value.Value
                        });
                    }
                }
                

                facetsOut.Add(facetJson);
            }

            return facetsOut;
        }

        public static List<ProductSearchSortableJson> ProductSortables(List<ProductSearchSortable> sortablesIn)
        {
            var sortablesOut = new List<ProductSearchSortableJson>();

            foreach (ProductSearchSortable sortableModel in sortablesIn)
            {
                var sortableJson = new ProductSearchSortableJson
                {
                    label = sortableModel.SortLabel,
                    orderByString =sortableModel.OrderByString
                };

                sortablesOut.Add(sortableJson);
            }

            return sortablesOut;
        }
    }
}