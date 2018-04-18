using InventoryHawk.Account.Public.Api.ApplicationImageRecordsService;
using InventoryHawk.Account.Public.Api.Transforms.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Dynamics
{
    public static class Products
    {
        public static List<IDictionary<string, object>> TransformDynamicProductsListForJson(List<Models.Search.Result> searchResults)
        {

            List<IDictionary<string, object>> products = new List<IDictionary<string, object>>();

            foreach(Models.Search.Result searchResult in searchResults)
            {
                IDictionary<string, object> product = TransformDynamicProductDetailsForJson(searchResult);
                products.Add(product);
            }

            return products;

        }

        public static IDictionary<string, object> TransformDynamicProductDetailsForJson(Models.Search.Result searchResult)
        {
            IDictionary<string, object> product = new System.Dynamic.ExpandoObject();

            product = searchResult.Document;
            product = LocationResultsTransforms.TransformLocationResults(product);
            product["images"] = searchResult.Images;


            return product;

        }
    }
}