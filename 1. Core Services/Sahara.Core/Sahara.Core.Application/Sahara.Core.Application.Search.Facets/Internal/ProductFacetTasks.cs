using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Sahara.Core.Accounts;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Search.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Facets.Internal
{
    internal static class ProductFacetTasks
    {
        internal static List<ProductSearchFacet> BuildProductFacets(string accountNameKey, bool includeHidden = true)
        {
            bool includeTags = true; //<--- Move to turn off in settings

            int facetMaximumResultCount = 80; //<--- 8 times the default of 10
            int facetMaximumResultCountNumbers = 40; //<--- 4 times the default of 10
            int facetMaximumResultCountSwatch = 80; //<--- 8 times the default of 10

            var productSearchFacets = new List<ProductSearchFacet>();

            //Get the account
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            //Get a list of all Properties
            var properties = PropertiesManager.GetProperties(account, PropertyListType.All);


            //Get client/index for this accounts search index 
            //var searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            //var accountSearchIndex = searchServiceClient.Indexes.GetClient(account.ProductSearchIndex);

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[account.ProductSearchIndex] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //Switched to accounts partitioned search client:
                var searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(account.SearchPartition);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(account.ProductSearchIndex);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(account.ProductSearchIndex, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }




            //Loop through and create facet request from Azure search for each property.searchFieldName
            var facets = new List<string>();
            foreach (PropertyModel property in properties)
            {
                if(property.Facetable)
                {
                    if (property.PropertyTypeNameKey == "number")// || property.PropertyTypeNameKey == "money" || property.PropertyTypeNameKey == "double")
                    {
                        if (property.FacetInterval > 0) //<-- We can override using ranges by setting this to 0
                        {
                            facets.Add(property.SearchFieldName + ",interval:" + property.FacetInterval);
                        }
                        else
                        {
                            facets.Add(property.SearchFieldName + ",count:" + facetMaximumResultCountNumbers);
                        }

                    }
                    else if(property.PropertyTypeNameKey == "location")
                    {
                        //Handle location data or ignore it
                    }
                    else if (property.PropertyTypeNameKey == "swatch")
                    {
                        facets.Add(property.SearchFieldName + ",count:" + facetMaximumResultCountSwatch);
                    }
                    else
                    {
                        facets.Add(property.SearchFieldName + ",count:" + facetMaximumResultCount);
                    }
                }        
            }

            if(includeTags)
            {
                facets.Add("tags,count: " + facetMaximumResultCount);
                properties.Add(new PropertyModel { PropertyName = "Tags", SearchFieldName = "tags", Facetable = true, AlwaysFacetable = true, PropertyTypeNameKey = "tags" });
            }

            //Prepare the query
            var searchParameters = new SearchParameters();
            searchParameters.Facets = facets;
            searchParameters.Top = 0; //<-- We just want the facets


            if(!includeHidden)
            {
                //Filter out all products that should not be publicly visble from their equivelant facet counts:
                searchParameters.Filter = "visible eq true";
            }

            //Get facet results from Azure Search (Perform the search)

            DocumentSearchResult searchresult = null;

            try
            {
               searchresult = accountSearchIndex.Documents.Search("", searchParameters);
            }
            catch(Exception e)
            {
                var str = e.Message;
            }

            //Loop through results and build out customized facet list FOR uiS TO USE
            //foreach(var facetResult in searchresult.Facets.AsEnumerable())
            //{
            //facetResult
            //}
            
            foreach (PropertyModel property in properties)
            {
                if (property.Facetable) // <-- Ensure we only handle the facetable properties first (ig
                {
                    //find the corresponding facet result if the type is not "location" or "swatch"
                    dynamic facetResult = null;
                    if (property.PropertyTypeNameKey != "location") // && property.PropertyTypeNameKey != "swatch")
                    {
                        facetResult = searchresult.Facets[property.SearchFieldName];
                    }
                    
                    //foreach (var facet in facetResult)
                    //{
                    var productSearchFacet = new ProductSearchFacet();
                    productSearchFacet.PropertyName = property.PropertyName;
                    productSearchFacet.SearchFieldName = property.SearchFieldName;
                    productSearchFacet.Values = new List<FacetValue>();
                    productSearchFacet.ShowAdvanced = false; //<-- Show or hide advanced options in the UI
                    productSearchFacet.BooleanOperator = "and"; //<-- "and" is default can also use "or"

                
                    if (property.PropertyTypeNameKey == "predefined")  // <-- Anything predefined we use as a facet automtically
                    {
                        #region build predefined facet

                        productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                        //productSearchFacet.FilterStyle = "checklist";

                        if (facetResult.Count >= 2) //<-- We only use this in search if there are 2 or more values
                        {
                            foreach (var facet in facetResult)
                            {
                                var name = facet.Value.ToString();

                                if (property.Symbol != null)
                                {
                                    #region Add symbol to value name

                                    string symbol = Common.Methods.Strings.ReplaceLeadingAndTrailingUnderscoresWithSpaces(property.Symbol);

                                    switch (property.SymbolPlacement)
                                    {
                                        case "leading":
                                            name = symbol + name;
                                            break;
                                        case "trailing":
                                            name = name + symbol;
                                            break;
                                    }

                                    #endregion
                                }

                                productSearchFacet.Values.Add(new FacetValue
                                {
                                    Name = name,
                                    Value = facet.Value.ToString(),
                                    Filter = property.SearchFieldName + "/any(s: s eq '" + facet.Value.ToString() + "')",
                                    Count = facet.Count.ToString()
                                });
                            }

                            productSearchFacets.Add(productSearchFacet);
                        }

                        #endregion
                    }
                    else if (property.PropertyTypeNameKey == "location")
                    {
                        //We add this in order to flag desire to search against this location property
                        productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                        productSearchFacet.Values = null;
                        productSearchFacet.BooleanOperator = null;
                        productSearchFacets.Add(productSearchFacet);
                    }
                    else if (property.PropertyTypeNameKey == "swatch")
                    {
                        #region build swatch facet

                        productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                        //productSearchFacet.FilterStyle = "checklist";

                        if (facetResult.Count >= 2) //<-- We only use this in search if there are 2 or more values
                        {
                            
                            foreach (var facet in facetResult)
                            {

                                #region Get the corresponding swatch image from the properties object

                                var swatchItem = property.Swatches.Find(i => i.PropertySwatchLabel == facet.Value);
                                var swatchUrl = swatchItem.PropertySwatchImage;

                                #endregion                 

                                var name = facet.Value.ToString();

                                productSearchFacet.Values.Add(new FacetValue
                                {
                                    Name = name,
                                    Value = facet.Value.ToString(),
                                    Filter = property.SearchFieldName + "/any(s: s eq '" + facet.Value.ToString() + "')",
                                    Count = facet.Count.ToString(),
                                    Image = swatchUrl

                                });
                            }

                            productSearchFacets.Add(productSearchFacet);
                        }

                        #endregion
                    }
                    else if (property.PropertyTypeNameKey == "tags")  
                    {
                        #region build tags facet

                        productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                        //productSearchFacet.FilterStyle = "checklist";

                        if (facetResult.Count >= 2) //<-- We only use this in search if there are 2 or more values
                        {
                            foreach (var facet in facetResult)
                            {
                                var name = facet.Value.ToString();

                                productSearchFacet.Values.Add(new FacetValue
                                {
                                    Name = name,
                                    Value = facet.Value.ToString(),
                                    Filter = property.SearchFieldName + "/any(t: t eq '" + facet.Value.ToString() + "')",
                                    Count = facet.Count.ToString()

                                });
                            }

                            productSearchFacets.Add(productSearchFacet);
                        }

                        #endregion
                    }
                    else if (property.PropertyTypeNameKey == "number")// || property.PropertyTypeNameKey == "money" || property.PropertyTypeNameKey == "double")
                    {
                        if(property.FacetInterval > 0) //<-- Create ranged facets
                        {
                            #region build numberic range facet with intervals

                            productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                            //productSearchFacet.FilterStyle = "checklist";

                            if (facetResult.Count >= 2) //<-- We only use this in search if there are 2 or more values
                            {
                                foreach (var facet in facetResult)
                                {
                                    var valueFrom = facet.Value; //facet.From.ToString();
                                    dynamic valueTo;                          

                                    #region Attempt to get nameTo value

                                    try
                                    {
                                        valueTo = (Int32.Parse(facet.Value.ToString())) + property.FacetInterval; //facet.To.ToString();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            valueTo = Decimal.Parse(facet.Value.ToString()) + property.FacetInterval; //facet.To.ToString();
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                valueTo = Double.Parse(facet.Value.ToString()) + property.FacetInterval; //facet.To.ToString();
                                            }
                                            catch
                                            {
                                                valueTo = float.Parse(facet.Value.ToString()) + property.FacetInterval; //facet.To.ToString();
                                            }
                                        }
                                    }

                                    #endregion

                                    string nameFrom = valueFrom.ToString();
                                    string nameTo = valueTo.ToString();

                                    #region Add commas

                                    if (nameFrom.Contains("."))
                                    {
                                        //Maintain Decimals (Not implemented)
                                    }
                                    else
                                    {
                                        try
                                        {
                                            nameFrom = (int.Parse(nameFrom).ToString("N0"));
                                        }
                                        catch
                                        {
                                        }
                                    }


                                    if (nameTo.Contains("."))
                                    {
                                        //Maintain Decimals (Not implemented)
                                    }
                                    else
                                    {
                                        try
                                        {
                                            nameTo = (int.Parse(nameTo).ToString("N0"));
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    #endregion

                                    if (property.Symbol != null)
                                    {
                                        #region Add symbol to value names

                                        string symbol = Common.Methods.Strings.ReplaceLeadingAndTrailingUnderscoresWithSpaces(property.Symbol);

                                        switch (property.SymbolPlacement)
                                        {
                                            case "leading":
                                                nameFrom = symbol + nameFrom;
                                                nameTo = symbol + nameTo;
                                                break;
                                            case "trailing":
                                                nameFrom = nameFrom + symbol;
                                                nameTo = nameTo + symbol;
                                                break;
                                        }

                                        #endregion
                                    }
                                    productSearchFacet.Values.Add(new FacetValue
                                    {
                                       Name = nameFrom + " - " + nameTo,
                                       Value = valueFrom + " - " + valueTo,
                                       Filter = property.SearchFieldName + " ge " + valueFrom + " and " + property.SearchFieldName + " lt " + valueTo,
                                       Count = facet.Count.ToString()
                                    });
                                }

                                productSearchFacets.Add(productSearchFacet);
                            }


                            #endregion
                        }
                        else //<-- Create a full listing (NOT DEFAULT BEHAVIOUR!)
                        {
                            #region build full listing for numerical facet

                            productSearchFacet.PropertyType = property.PropertyTypeNameKey;
                            //productSearchFacet.FilterStyle = "checklist";

                            if (facetResult.Count >= 2) //<-- We only use this in search if there are 2 or more values
                            {
                                foreach (var facet in facetResult)
                                {
                                    productSearchFacet.Values.Add(new FacetValue
                                    {
                                        Name = facet.Value.ToString(),
                                        Value = facet.Value.ToString(),
                                        Filter = property.SearchFieldName + " eq " + facet.Value.ToString(),
                                        Count = facet.Count.ToString()
                                    });
                                }

                                productSearchFacets.Add(productSearchFacet);
                            }

                            #endregion
                        }
                    }
                }
            }


            return productSearchFacets;
        }
    }
}
