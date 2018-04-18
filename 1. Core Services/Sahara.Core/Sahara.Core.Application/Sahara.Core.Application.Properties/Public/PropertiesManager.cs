using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Properties.Internal;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Search;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties
{
    public static class PropertiesManager
    {
        #region Global

        public static List<PropertyTypeModel> GetPropertyTypes()
        {
            List<PropertyTypeModel> propertyTypes = null;

            string cacheKey = GlobalHash.Key;
            string cacheField = GlobalHash.Fields.PropertyTypes;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            #region Get propertyTypes from cache

            try
            {
                var redisValue = cache.HashGet(cacheKey, cacheField);
                if (redisValue.HasValue)
                {
                    propertyTypes = JsonConvert.DeserializeObject<List<PropertyTypeModel>>(redisValue);
                }
            }
            catch
            {

            }

            #endregion
            
            if (propertyTypes == null)
            {
                #region Get categories from SQL

                propertyTypes = Sql.Statements.SelectStatements.SelectPropertyTypeList();

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(cacheKey, cacheField, JsonConvert.SerializeObject(propertyTypes), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
                

                #endregion
            }

            return propertyTypes;
        }


        #endregion

        #region Create

        public static DataAccessResponseType CreateProperty(Account account, string propertyTypeNameKey, string propertyName)
        {
            //TO DO: Always clear/update caches AND update counts!

            var response = new DataAccessResponseType();

            #region Validate Property Name:

            ValidationResponseType propertyNameValidationResponse = ValidationManager.IsValidPropertyName(propertyName);
            if (!propertyNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = propertyNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(propertyNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            var property = new PropertyModel
            {
                PropertyTypeNameKey = propertyTypeNameKey,
                PropertyID = Guid.NewGuid(),
                PropertyName = propertyName,
                PropertyNameKey = Sahara.Core.Common.Methods.PropertyNames.ConvertToPropertyNameKey(propertyName),
                SearchFieldName = Sahara.Core.Common.Methods.PropertyNames.ConvertToPropertyNameKey(propertyName, true),
                Facetable = false,
                Sortable = false,
                Listing = false,
                Details = true
            };

            #region Check if this name already exists

            var propertyExists = GetProperty(account, property.PropertyNameKey);

            if (propertyExists != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A property with that name already exists.";

                return response;
            }

            #endregion



            response = Sql.Statements.InsertStatements.InsertProperty(account.SqlPartition, account.SchemaName, property);

            var searchUpdateResponse = new DataAccessResponseType();

            if (response.isSuccess)
            {
                //Clear Category Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);

                //Update Search Index Fields
                searchUpdateResponse = ProductSearchManager.AddProductPropertyToSearchIndexFields(account.SearchPartition, account.ProductSearchIndex, property.SearchFieldName, propertyTypeNameKey);

            }

            if(!searchUpdateResponse.isSuccess)
            {
                #region Rollback and send Error

                //Delete the property
                Sql.Statements.DeleteStatements.DeleteProperty(account.SqlPartition, account.SchemaName, property.PropertyID.ToString());

                //Return the error
                return searchUpdateResponse;

                #endregion
            }

            //Clear Caches & Send Response:
            Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            response.SuccessMessage = property.PropertyID.ToString(); //<--Returned for logging purposes

            return response;

        }

        public static DataAccessResponseType CreatePropertyValue(Account account, PropertyModel property, string propertyValueName)
        {
            //TO DO: Always clear/update caches AND update counts!


            #region Verify value

            if (propertyValueName == null || propertyValueName == "")
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Value must have a value!"
                };
            }

            if (propertyValueName.Length > 60)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Values cannot be longer than 60 characters!"
                };
            }

            #endregion



            var response = new DataAccessResponseType();

            #region Validate Property Value Name (Depricate ???)
            /*
            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(propertyValueName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }
            */
            #endregion

            #region Validate Property can have values

            if(property.PropertyTypeNameKey != "predefined")
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This property cannot be assigned predefined values"
                };
            }

            #endregion

            #region Verify value does not already exist on this property

            foreach (PropertyValueModel propertyValueModel in property.Values)
            {
                if(propertyValueModel.PropertyValueNameKey == Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyValueName))
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "This property already has this value assigned."
                    };
                }
            }

            #endregion

            var propertyValue = new PropertyValueModel
            {
                PropertyID = property.PropertyID,
                PropertyValueID = Guid.NewGuid(),
                PropertyValueName = propertyValueName,
                PropertyValueNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyValueName),
                Visible = true
            };

            response = Sql.Statements.InsertStatements.InsertPropertyValue(account.SqlPartition, account.SchemaName, propertyValue);

            if (response.isSuccess)
            {
                //Clear Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            response.SuccessMessage = propertyValue.PropertyValueID.ToString(); //<--Returned for logging purposes

            return response;

        }

        #region Swatches

        /// <summary>
        /// Step one, upload image
        /// Step 2 requires submiting this URl with the label and property info for the new swatch
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="byteArray"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public static DataAccessResponseType UploadSwatchImage(string accountId, string storagePartition, byte[] byteArray, out string imageUrl)
        {
            var response = new DataAccessResponseType();
            imageUrl = null;

            //Verify, Edit & Store the image:
            var swatchPropertyImageProcessor = new Imaging.SwatchPropertyImageProcessor();
            response = swatchPropertyImageProcessor.ProcessSwatchImage(accountId, storagePartition, byteArray);

            if (!response.isSuccess)
            {
                return response; //<--return error
            }

            imageUrl = response.SuccessMessage;

            return response;
        }

        //Step 2
        public static DataAccessResponseType CreatePropertySwatch(Account account, PropertyModel property, string swatchImageUrl, string swatchLabelName)
        {
            //TO DO: Always clear/update caches AND update counts!

            #region Verify values
            if (swatchImageUrl == null || swatchImageUrl == "")
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Swatch must have an image!"
                };
            }
            if (swatchLabelName == null || swatchLabelName == "")
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Label must have a value!"
                };
            }
            if (swatchLabelName.Length > 24)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Labels cannot be longer than 24 characters!"
                };
            }
            if (swatchLabelName.Contains("|"))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Labels cannot contain the pipe character."
                };
            }

            #endregion

            var response = new DataAccessResponseType();

            #region Validate Property Value Name (Depricate ???)
            /*
            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(propertyValueName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }
            */
            #endregion

            #region Validate Property is a swatch

            if (property.PropertyTypeNameKey != "swatch")
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This property cannot be assigned swatch values"
                };
            }

            #endregion

            #region Verify value does not already exist on this property

            foreach (PropertySwatchModel propertySwatchModel in property.Swatches)
            {
                if (propertySwatchModel.PropertySwatchNameKey == Common.Methods.ObjectNames.ConvertToObjectNameKey(swatchLabelName))
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "This swatch already has this label assigned."
                    };
                }
            }

            #endregion

            var propertySwatch = new PropertySwatchModel
            {
                PropertyID = property.PropertyID,
                PropertySwatchImage = swatchImageUrl,
                PropertySwatchImageMedium = swatchImageUrl.Replace(".jpg", "_md.jpg"),
                PropertySwatchImageSmall = swatchImageUrl.Replace(".jpg", "_sm.jpg"),
                PropertySwatchID = Guid.NewGuid(),
                PropertySwatchLabel = swatchLabelName,
                PropertySwatchNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(swatchLabelName),
                Visible = true
            };

            response = Sql.Statements.InsertStatements.InsertPropertySwatch(account.SqlPartition, account.SchemaName, propertySwatch);

            if (response.isSuccess)
            {
                //Clear Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            response.SuccessMessage = propertySwatch.PropertySwatchID.ToString(); //<--Returned for logging purposes

            return response;

        }

        #endregion

        #endregion

        #region Get

        public static PropertyModel GetProperty(Account account, string propertyNameKey, bool useCachedVersion = true)
        {
            PropertyModel property = null;

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            redisHashField = ApplicationPropertiesHash.Fields.Details(Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyNameKey));

            #endregion


            if (useCachedVersion)
            {
                #region Get property from cache (if user requests by name)

                try
                {
                    var redisValue = cache.HashGet(ApplicationPropertiesHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        property = JsonConvert.DeserializeObject<PropertyModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (property == null)
            {
                #region Get property from SQL

                property = Sql.Statements.SelectStatements.SelectProperty(account.SqlPartition, account.SchemaName, propertyNameKey);

                #endregion

                if (property != null)
                {
                    #region Store property into cache (using short names ONLY)

                    try
                    {
                        cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey),
                            ApplicationPropertiesHash.Fields.Details(property.PropertyNameKey),
                            JsonConvert.SerializeObject(property), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }

            }

            return property;
        }


        public static List<PropertyModel> GetProperties(Account account, PropertyListType listType, bool useCachedVersion = true)
        {
            List<PropertyModel> properties = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = ApplicationPropertiesHash.Fields.All();
         
            switch(listType)
            {
                case PropertyListType.All:
                    redisHashField = ApplicationPropertiesHash.Fields.All();
                    break;
                case PropertyListType.Listings:
                    redisHashField = ApplicationPropertiesHash.Fields.Listings();
                    break;
                case PropertyListType.Details:
                    redisHashField = ApplicationPropertiesHash.Fields.Details();
                    break;
                case PropertyListType.Featured:
                    redisHashField = ApplicationPropertiesHash.Fields.Featured();
                    break;
            }
            


            if (useCachedVersion)
            {
                #region Get properties from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationPropertiesHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        properties = JsonConvert.DeserializeObject<List<PropertyModel>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (properties == null)
            {
                #region Get properties from SQL

                properties = Sql.Statements.SelectStatements.SelectPropertyList(account.SqlPartition, account.SchemaName);

                #region Append account cdn link to all swatch images

                var cdnEndpoint = Core.Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;

                foreach (var property in properties)
                {
                    if(property.PropertyTypeNameKey == "swatch")
                    {
                        foreach(var swatch in property.Swatches)
                        {
                            swatch.PropertySwatchImage = "https://" + cdnEndpoint + "/" + swatch.PropertySwatchImage;
                            swatch.PropertySwatchImageMedium = "https://" + cdnEndpoint + "/" + swatch.PropertySwatchImageMedium;
                            swatch.PropertySwatchImageSmall = "https://" + cdnEndpoint + "/" + swatch.PropertySwatchImageSmall;
                        }
                    }
                }

                #endregion

                #endregion

                #region Store properties into cache (by all, listing & details)

                #region build seperate property lists

                var listingProperties = new List<PropertyModel>();
                var detailProperties = new List<PropertyModel>();
                var feturedProperties = new List<PropertyModel>();

                foreach (var property in properties)
                {
                    if(property.Listing)
                    {
                        listingProperties.Add(property);
                    }
                    if (property.Details)
                    {
                        detailProperties.Add(property);
                    }
                    if (property.FeaturedID > 0)
                    {
                        feturedProperties.Add(property);
                    }
                }

                listingProperties = listingProperties.OrderBy(o => o.OrderID).ToList();
                detailProperties = detailProperties.OrderBy(o => o.OrderID).ToList();
                feturedProperties = feturedProperties.OrderBy(o => o.FeaturedID).ToList();

                #endregion

                try
                {
                    //All
                    cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey), ApplicationPropertiesHash.Fields.All(), JsonConvert.SerializeObject(properties), When.Always, CommandFlags.FireAndForget);

                    //Listing
                    cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey), ApplicationPropertiesHash.Fields.Listings(), JsonConvert.SerializeObject(listingProperties), When.Always, CommandFlags.FireAndForget);

                    //Details
                    cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey), ApplicationPropertiesHash.Fields.Details(), JsonConvert.SerializeObject(detailProperties), When.Always, CommandFlags.FireAndForget);

                    //Featured
                    cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey), ApplicationPropertiesHash.Fields.Featured(), JsonConvert.SerializeObject(feturedProperties), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion

                //Assign the correct propertyListType to the return object:
                switch (listType)
                {
                    case PropertyListType.Listings:
                        properties = listingProperties;
                        break;
                    case PropertyListType.Details:
                        properties = detailProperties;
                        break;
                    case PropertyListType.Featured:
                        properties = feturedProperties;
                        break;
                }
            }

            return properties;
        }

        public static int GetPropertyCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationPropertiesHash.Fields.Count();

            if (useCachedVersion)
            {
                #region Get count from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationPropertiesHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectPropertyCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache (by Name only)

                try
                {
                    cache.HashSet(ApplicationPropertiesHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
                
                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }


            return count;
        }

        #endregion

        #region Update

        public static DataAccessResponseType UpdatePropertyListingState(Account account, PropertyModel property, bool listingState)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyListingState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), listingState);

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertyDetailsState(Account account, PropertyModel property, bool detailsState)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyDetailsState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), detailsState);

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertyFacetInterval(Account account, PropertyModel property, int newFacetInterval)
        {

            var response = new DataAccessResponseType();

            if(property.PropertyTypeNameKey == "number")// || property.PropertyTypeNameKey == "money" || property.PropertyTypeNameKey == "double")
            {
                response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyFacetInterval(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), newFacetInterval);
            }
            else
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Only numerical property types use facet ranges."
                };
            }

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertyFacetableState(Account account, PropertyModel property, bool isFacetable)
        {
            if(property.PropertyTypeNameKey == "paragraph")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the facetability of paragraph types" };
            }
            else if (property.PropertyTypeNameKey == "string")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the facetability of string types" };
            }

            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyFacetableState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), isFacetable);

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertySortableState(Account account, PropertyModel property, bool isSortable)
        {
            if (property.PropertyTypeNameKey == "paragraph")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the sortability of paragraph types" };
            }
            else if (property.PropertyTypeNameKey == "swatch")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the sortability of swatch types" };
            }
            else if (property.PropertyTypeNameKey == "predefined")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the sortability of predefined types" };
            }
            else if (property.PropertyTypeNameKey == "location")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot update the sortability of location types" };
            }

            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertySortableState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), isSortable);

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertyAppendableState(Account account, PropertyModel property, bool isAppendable)
        {
            if (property.PropertyTypeNameKey == "predefined" || property.PropertyTypeNameKey == "swatch")
            {
                var response = new DataAccessResponseType();

                #region If going From Appendable to Unappendable - first make sure that no documents have more than one item in any array representing this property
                
                if(isAppendable == false)
                {
                    //Get the DocumentDB Client
                    //var client = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient;
                    //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                    //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

                    //string sqlQuery = "SELECT Top 1 * FROM Products p WHERE ARRAY_CONTAINS(p.Properties, '" + property.PropertyName + "')";

                    string sqlQuery = String.Empty;

                    switch(property.PropertyTypeNameKey)
                    {
                        case "predefined":
                            sqlQuery = "SELECT Top 1 * FROM Products p WHERE ARRAY_LENGTH(p.Predefined['" + property.PropertyName + "']) > 1";
                            break;
                        case "swatch":
                            sqlQuery = "SELECT Top 1 * FROM Products p WHERE ARRAY_LENGTH(p.Swatches['" + property.PropertyName + "']) > 1";
                            break;
                    }                   

                    //Build a collection Uri out of the known IDs
                    //(These helpers allow you to properly generate the following URI format for Document DB:
                    //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
                    //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

                    var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();

                    if (document != null)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Cannot change appendable state to false while a product has multiple items associted. Please limit all property associations to 1 per product before updating appendability."
                        };
                    }
                }
                
                #endregion

                response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyAppendableState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), isAppendable);

                if (response.isSuccess)
                {
                    //Clear Property Caches:
                    Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
                }

                return response;
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You may only update the appendability of predefined or swatch property types" };
            }

        }

        #region Removed

        /*
        public static DataAccessResponseType UpdatePropertyHighlightedState(Account account, PropertyModel property, bool isHighlighted)
        {
            if (property.PropertyTypeNameKey == "paragraph")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot make paragraph types highlighted properties" };
            }

            var properties = GetProperties(account, PropertyListType.All);

            foreach(var propertyModel in properties)
            {
                if(propertyModel.Highlighted)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You may only have 1 highlited property at a time" };
                }
            }

            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertyHighlightedState(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), isHighlighted);

            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }
        */

        #endregion

        public static DataAccessResponseType UpdatePropertySymbol(Account account, PropertyModel property, string symbol)
        {
            var response = new DataAccessResponseType();

            if (symbol.Length > 8)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Symbols cannot be more than 8 characters"
                };
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertySymbol(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), symbol);
            
            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdatePropertySymbolPlacement(Account account, PropertyModel property, string symbolPlacement)
        {
            var response = new DataAccessResponseType();

            if (symbolPlacement.ToLower() == "leading" || symbolPlacement.ToLower() == "trailing")
            {
                //Do nothing
            }
            else
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Symbol placement can only be 'leading' or 'trailing'"
                };
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdatePropertySymbolPlacement(account.SqlPartition, account.SchemaName, property.PropertyID.ToString(), symbolPlacement.ToLower());
            
            if (response.isSuccess)
            {
                //Clear Property Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #region Featured Properties

        public static DataAccessResponseType UpdateFeaturedProperties(Account account, Dictionary<string, int> featuredOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.SetFeaturedProperties(account.SqlPartition, account.SchemaName, featuredOrderingDictionary);

            if (response.isSuccess)
            {
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ResetFeaturedProperties(Account account)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ResetFeaturedProperties(account.SqlPartition, account.SchemaName);

            if (response.isSuccess)
            {
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;
        }


        #endregion

        #region Delete
        // Not implemented until Azure Search allows us to remove fields from indexes
        public static DataAccessResponseType DeleteProperty(Account account, PropertyModel property)
        {
            var response = new DataAccessResponseType();

            /*
            #region Check if property is currently in use on ANY documents for this account

            #region Get any relevant documents (Legacy - Switch to AzureSearch)

            //Get the DocumentDB Client
            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            string sqlQuery = "SELECT Top 1 * FROM Products p WHERE ARRAY_CONTAINS(p.Properties, '" + property.PropertyName + "')";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = client.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();

            #endregion

            #endregion

            if (document != null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a property that is in use on a product. Please remove all property associations before deleting."
                };
            }
            else
            {
                response.isSuccess = Sql.Statements.DeleteStatements.DeleteProperty(account.SqlPartition, account.SchemaName, property.PropertyID.ToString());
            }

            if (response.isSuccess)
            {
                //Clear Category Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);

                //Delete on Search 
                ProductSearchManager.RemoveProductSearchIndexField(account.ProductSearchIndex, property.PropertyName);
            }
            */
            return response;

        }

        public static DataAccessResponseType DeletePropertyValue(Account account, PropertyModel property, PropertyValueModel propertyValue)
        {
            var response = new DataAccessResponseType();

            #region Check if property value is currently in use on ANY documents for this account

            #region Get any relevant documents (Legacy)
            /*
            //Get the DocumentDB Client
            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            string sqlQuery = "SELECT Top 1 * FROM Products p WHERE p.Properties." + property.PropertyName + " = '" + propertyValue.PropertyValueName + "'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = client.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();
            */
            #endregion

            #endregion

            #region Get any relevant documents (from Search)

            //$filter=tags/any(t: t eq '345')
            string searchFilter = property.SearchFieldName + "/any(s: s eq '" + propertyValue.PropertyValueName + "')";

            var documentSearchResult = ProductSearchManager.SearchProducts(account.SearchPartition, account.ProductSearchIndex, "", searchFilter, "relevance", 0, 1);

            #endregion

            if (documentSearchResult.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a property value that is in use on a product. Please remove all associations before deleting."
                };
            }
            else
            {
                response.isSuccess = Sql.Statements.DeleteStatements.DeletePropertyValue(account.SqlPartition, account.SchemaName, propertyValue.PropertyValueID.ToString());
            }

            if (response.isSuccess)
            {
                //Clear Category Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);
            }

            return response;

        }

        public static DataAccessResponseType DeletePropertySwatch(Account account, PropertyModel property, PropertySwatchModel propertySwatch)
        {
            var response = new DataAccessResponseType();

            #region Check if property value is currently in use on ANY documents for this account

            #region Get any relevant documents (Legacy)

            /*
            //Get the DocumentDB Client
            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            string sqlQuery = "SELECT Top 1 * FROM Products p WHERE p.Swatches." + property.PropertyName + " = '" + propertySwatch.PropertySwatchLabel + "'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = client.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();
            */
            #endregion

            #region Get any relevant documents (from Search)

            //$filter=tags/any(t: t eq '345')
            string searchFilter = property.SearchFieldName + "/any(s: s eq '" + propertySwatch.PropertySwatchLabel + "')";

            var documentSearchResult = ProductSearchManager.SearchProducts(account.SearchPartition, account.ProductSearchIndex, "", searchFilter, "relevance", 0, 1);

            #endregion

            #endregion

            if (documentSearchResult.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a swatch that is in use on a product. Please remove all associations before deleting."
                };
            }
            else
            {
                response.isSuccess = Sql.Statements.DeleteStatements.DeletePropertySwatch(account.SqlPartition, account.SchemaName, propertySwatch.PropertySwatchID.ToString());
            }

            if (response.isSuccess)
            {
                //Clear Caches:
                Caching.InvalidateAllPropertyCachesForAccount(account.AccountNameKey);

                #region Delete all associated images for the swatch being deleted

                /*
                try
                {
                   //TODO (Really not very important)
                }
                catch
                {

                }
                */
                #endregion
            }

            return response;

        }

        #endregion
    }
}

