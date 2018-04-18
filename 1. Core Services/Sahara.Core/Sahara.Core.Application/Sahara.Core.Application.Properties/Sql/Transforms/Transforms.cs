using Sahara.Core.Application.Properties.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Sql
{
    public static class Transforms
    {
        public static PropertyTypeModel DataReader_to_PropertyTypeModel(SqlDataReader reader)
        {
            PropertyTypeModel propertyType = new PropertyTypeModel();

            propertyType.PropertyTypeID = (Guid)reader["PropertyTypeID"];
            propertyType.PropertyTypeName = (String)reader["PropertyTypeName"];
            propertyType.PropertyTypeNameKey = (String)reader["PropertyTypeNameKey"];
            propertyType.PropertyTypeDescription = (String)reader["PropertyTypeDescription"];

            return propertyType;
        }

        public static PropertyModel DataReader_to_PropertyModel(SqlDataReader reader)
        {
            PropertyModel property = new PropertyModel();

            property.PropertyTypeNameKey = (String)reader["PropertyTypeNameKey"];
            property.PropertyID = (Guid)reader["PropertyID"];
            property.PropertyName = (String)reader["PropertyName"];
            property.PropertyNameKey = (String)reader["PropertyNameKey"];
            property.SearchFieldName = (String)reader["SearchFieldName"];
            property.FacetInterval = (int)reader["FacetInterval"];
            property.OrderID = (int)reader["OrderID"];
            property.FacetOrderID = (int)reader["FacetOrderID"];
            //property.ListingOrderID = (int)reader["ListingOrderID"];
            //property.DetailsOrderID = (int)reader["DetailsOrderID"];
            property.Listing = (bool)reader["Listing"];
            property.Details = (bool)reader["Details"];
            property.AlwaysFacetable = (bool)reader["AlwaysFacetable"];
            property.Facetable = (bool)reader["Facetable"];
            property.Sortable = (bool)reader["Sortable"];
            property.Appendable = (bool)reader["Appendable"];
            //property.Highlighted = (bool)reader["Highlighted"];
            property. FeaturedID = (int)reader["FeaturedID"];
            property.CreatedDate = (DateTime)reader["CreatedDate"];

            property.Symbol = null;
            property.SymbolPlacement = null;

            if (reader["Symbol"] != DBNull.Value)
            {
                property.Symbol = (String)reader["Symbol"];
            }
            if (reader["SymbolPlacement"] != DBNull.Value)
            {
                property.SymbolPlacement = (String)reader["SymbolPlacement"];
            }
            if (reader["NumericDescriptor"] != DBNull.Value)
            {
                property.NumericDescriptor = (String)reader["NumericDescriptor"];
            }
            return property;
        }

        public static PropertyValueModel DataReader_to_PropertyValueModel(SqlDataReader reader)
        {
            PropertyValueModel propertyValue = new PropertyValueModel();

            propertyValue.PropertyID = (Guid)reader["PropertyID"];

            propertyValue.PropertyValueID = (Guid)reader["PropertyValueID"];
            propertyValue.PropertyValueName = (String)reader["PropertyValueName"];
            propertyValue.PropertyValueNameKey = (String)reader["PropertyValueNameKey"];
            propertyValue.OrderID = (int)reader["OrderID"];
            propertyValue.Visible = (bool)reader["Visible"];
            propertyValue.CreatedDate = (DateTime)reader["CreatedDate"];

            return propertyValue;
        }

        public static PropertySwatchModel DataReader_to_PropertySwatchModel(SqlDataReader reader)
        {
            PropertySwatchModel propertySwatch = new PropertySwatchModel();

            propertySwatch.PropertyID = (Guid)reader["PropertyID"];

            propertySwatch.PropertySwatchID = (Guid)reader["PropertySwatchID"];

            propertySwatch.PropertySwatchImage = (String)reader["PropertySwatchImage"];
            propertySwatch.PropertySwatchImageMedium = (String)reader["PropertySwatchImageMedium"];
            propertySwatch.PropertySwatchImageSmall = (String)reader["PropertySwatchImageSmall"];

            propertySwatch.PropertySwatchLabel = (String)reader["PropertySwatchLabel"];
            propertySwatch.PropertySwatchNameKey = (String)reader["PropertySwatchNameKey"];
            propertySwatch.OrderID = (int)reader["OrderID"];
            propertySwatch.Visible = (bool)reader["Visible"];
            propertySwatch.CreatedDate = (DateTime)reader["CreatedDate"];

            return propertySwatch;
        }
    }


}
