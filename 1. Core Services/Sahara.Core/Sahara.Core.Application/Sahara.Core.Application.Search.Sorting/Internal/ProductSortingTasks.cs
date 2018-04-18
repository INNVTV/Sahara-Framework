using Sahara.Core.Accounts;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Search.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Sorting.Internal
{
    internal static class ProductSortingTasks
    {
        internal static List<ProductSearchSortable> BuildProductSortables(string accountNameKey)
        {
            
            var productSearchSortables = new List<ProductSearchSortable>();

            //Get the account
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            //Get a list of all Properties
            var properties = PropertiesManager.GetProperties(account, PropertyListType.All);

            
            
            // ------------- Default sortables (Bookend) -----------------

            productSearchSortables.Add(new ProductSearchSortable {
                SortLabel = "Relevance",
                OrderByString = "relevance",
            });

            // ------------- Default sortables (Second Tier) -----------------

            productSearchSortables.Add(new ProductSearchSortable
            {
                SortLabel = "Date added (Newest)",
                OrderByString = "dateCreated desc",
            });

            productSearchSortables.Add(new ProductSearchSortable
            {
                SortLabel = "Date added (Oldest)",
                OrderByString = "dateCreated",
            });

            productSearchSortables.Add(new ProductSearchSortable
            {
                SortLabel = "Name (A-Z)",
                OrderByString = "name",
            });

            productSearchSortables.Add(new ProductSearchSortable
            {
                SortLabel = "Name (Z-A)",
                OrderByString = "name desc",
            });

            //SortLabel = "Price (Low - High)",


            // ------------- Dynamic sortables -----------------

            //Loop through and create facet request from Azure search for each property.searchFieldName
            var facets = new List<string>();
            foreach (PropertyModel property in properties)
            {
                if(property.Sortable)
                {
                    if(property.PropertyTypeNameKey == "datetime")
                    {
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (Newest)",
                            OrderByString = property.SearchFieldName + " desc"
                        });
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (Oldest)",
                            OrderByString = property.SearchFieldName
                        });
                    }
                    else if (property.PropertyTypeNameKey == "string" || property.PropertyTypeNameKey == "predefined")
                    {
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (A-Z)",
                            OrderByString = property.SearchFieldName
                        });
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (Z-A)",
                            OrderByString = property.SearchFieldName + " desc"
                        });
                    }
                    else if (property.PropertyTypeNameKey == "number") // || property.PropertyTypeNameKey == "money" || property.PropertyTypeNameKey == "double")
                    {
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (High-Low)",
                            OrderByString = property.SearchFieldName + " desc"
                        });
                        productSearchSortables.Add(new ProductSearchSortable
                        {
                            SortLabel = property.PropertyName + " (Low-High)",
                            OrderByString = property.SearchFieldName
                        });
                    }
                }

                if (property.PropertyTypeNameKey == "number") // || property.PropertyTypeNameKey == "money" || property.PropertyTypeNameKey == "double")
                {
                    if (property.FacetInterval > 0) //<-- We can override using ranges by setting this to 0
                    {
                        facets.Add(property.SearchFieldName + ",interval:" + property.FacetInterval);
                    }
                    else
                    {
                        facets.Add(property.SearchFieldName);
                    }

                }
                else
                {
                    facets.Add(property.SearchFieldName);
                }

            }






            return productSearchSortables;
        }
    }
}
