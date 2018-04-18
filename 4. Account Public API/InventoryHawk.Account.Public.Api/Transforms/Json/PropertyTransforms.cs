using InventoryHawk.Account.Public.Api.ApplicationPropertiesService;
using InventoryHawk.Account.Public.Api.Models.Json.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Transforms.Json
{
    public class PropertyTransforms
    {
        public static PropertiesResultJson Properties(string accountNameKey, List<PropertyModel> propertiesIn, string filter = null)
        {
            var propertyJsonOut = new PropertiesResultJson();

            propertyJsonOut.properties = new List<PropertyJson>();

            foreach (PropertyModel property in propertiesIn)
            {
                var propertyJson = new PropertyJson
                {
                    propertyName = property.PropertyName,
                    propertyNameKey = property.PropertyNameKey,
                    propertyType = property.PropertyTypeNameKey,
                    searchField = property.SearchFieldName
                };

                if (property.PropertyTypeNameKey == "number" && property.Symbol != null)
                {
                    propertyJson.symbol = new PropertySymbolJson
                    {
                        value = property.Symbol,
                        placement = property.SymbolPlacement
                    };
                }

                if (property.PropertyTypeNameKey == "swatch" && property.Swatches != null)
                {
                    propertyJson.swatches = new List<PropertySwatchesJson>();

                    foreach (PropertySwatchModel swatch in property.Swatches)
                    {
                        propertyJson.swatches.Add(new PropertySwatchesJson
                        {
                            label = swatch.PropertySwatchLabel,
                            image = swatch.PropertySwatchImage,
                            imageMedium = swatch.PropertySwatchImageMedium,
                            imageSmall = swatch.PropertySwatchImageSmall
                        });
                    }

                }

                if (filter == "listingsOnly" && property.Listing)
                {
                    propertyJsonOut.properties.Add(propertyJson);
                }
                else if(filter == null)
                {
                    propertyJsonOut.properties.Add(propertyJson);
                }

                
            }

            propertyJsonOut.count = propertyJsonOut.properties.Count();

            return propertyJsonOut;
        }

        public static PropertyResultJson Property(string accountNameKey, PropertyModel propertyIn)
        {
            var propertyJsonOut = new PropertyResultJson();

            propertyJsonOut.property = new PropertyJson();


                var propertyJson = new PropertyJson
                {
                    propertyName = propertyIn.PropertyName,
                    propertyNameKey = propertyIn.PropertyNameKey,
                    propertyType = propertyIn.PropertyTypeNameKey,
                    searchField = propertyIn.SearchFieldName
                };

                if (propertyIn.PropertyTypeNameKey == "number" && propertyIn.Symbol != null)
                {
                    propertyJson.symbol = new PropertySymbolJson
                    {
                        value = propertyIn.Symbol,
                        placement = propertyIn.SymbolPlacement
                    };
                }

                if (propertyIn.PropertyTypeNameKey == "swatch" && propertyIn.Swatches != null)
                {
                    propertyJson.swatches = new List<PropertySwatchesJson>();

                    foreach (PropertySwatchModel swatch in propertyIn.Swatches)
                    {
                        propertyJson.swatches.Add(new PropertySwatchesJson
                        {
                            label = swatch.PropertySwatchLabel,
                            image = swatch.PropertySwatchImage,
                            imageMedium = swatch.PropertySwatchImageMedium,
                            imageSmall = swatch.PropertySwatchImageSmall
                        });
                    }

                }

                propertyJsonOut.property = (propertyJson);


            return propertyJsonOut;
        }
    }
}