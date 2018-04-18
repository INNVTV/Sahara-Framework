using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Transforms.Json
{
    //Used for location merging
    class locationPropertyObject
    {
        public string key { get; set; }
        public IDictionary<string, object> obj { get; set; }
    }

    public static class LocationResultsTransforms
    {
        public static IDictionary<string, object> TransformLocationResults(IDictionary<string, object> product)
        {
            #region Check if any location properties exist and MERGE with parent location data

            //Hold for removal AFTER loop
            List<string> propertiesToRemove = new List<string>();

            //Used for updates
            List<locationPropertyObject> propertiesToUpdate = new List<locationPropertyObject>();

            //Merge
            foreach (var property in product)
            {
                if (property.Key.Contains("LocationMetadata"))
                {
                    var parentKey = property.Key.Replace("LocationMetadata", "");

                    if (product[parentKey] != null && property.Value != null)
                    {

                        //Split metadata string into array:
                        var locationMetadataArray = property.Value.ToString().Split(new string[] { " || " }, StringSplitOptions.None);

                        IDictionary<string, object> locationMetadata = new System.Dynamic.ExpandoObject();

                        var geoData = (Microsoft.Spatial.GeographyPoint)product[parentKey];

                        locationMetadata["name"] = locationMetadataArray[0];
                        locationMetadata["address1"] = locationMetadataArray[1];
                        locationMetadata["address2"] = locationMetadataArray[2];
                        locationMetadata["city"] = locationMetadataArray[3];
                        locationMetadata["state"] = locationMetadataArray[4];
                        locationMetadata["postalCode"] = locationMetadataArray[5];
                        locationMetadata["country"] = locationMetadataArray[6];

                        locationMetadata["geospatial"] = geoData;

                        //product[property.Key] = locationMetadata;
                        propertiesToUpdate.Add(new locationPropertyObject { key = parentKey, obj = locationMetadata });


                    }

                    //Add to array to remove from properties
                    propertiesToRemove.Add(property.Key);

                }
            }

            //Merge
            foreach (var obj in propertiesToUpdate)
            {
                product[obj.key] = obj.obj;
            }

            //Remove:
            foreach (string propertyName in propertiesToRemove)
            {
                product.Remove(propertyName);
            }

            #endregion

            return product;
        }
    }
}