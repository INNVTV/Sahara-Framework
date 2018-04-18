-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[Property](


		[PropertyTypeNameKey]		NVARCHAR (120) NOT NULL,
		[PropertyID]				[uniqueidentifier] NOT NULL,

		[PropertyName]				NVARCHAR (120) NOT NULL,
		[PropertyNameKey]			NVARCHAR (120) NOT NULL,
		[SearchFieldName]			NVARCHAR (120) NOT NULL,

		[Symbol]					NVARCHAR (8) NULL, -- If not NULL Placed in front or behind values ('$', 'ft' '"', 'sq.' etc....) - Mostly used for search facets and product detail page when showing value on numbers,  money or decimals
		[SymbolPlacement]			NVARCHAR (8) DEFAULT 'leading', -- "leading" or "trailing"
		[NumericDescriptor]			NVARCHAR (12) NULL,  -- If not NULL Placed behind values ('per pound' 'a square foot' etc....) - Mostly used for search facets and product detail page when showing value on numbers,  money or decimals

		[Facetable]					BIT NOT NULL DEFAULT 1, -- Determines weather or not a property will be used as a facet for search
		[AlwaysFacetable]			BIT NOT NULL DEFAULT 0, -- If true this property will be used as a facet regardless of built in logic. Must be changed via SQL manually.		
		[FacetInterval]				INT NOT NULL DEFAULT 100, -- Used to determin ranges for facets on numerical ranges

		[Appendable]				BIT NOT NULL DEFAULT 0, -- Determines weather or not a property can be appended to (must be of type "collection" in Azure Search and of PropertyType "predefined" or "swatch"

		[Sortable]					BIT NOT NULL DEFAULT 0, -- Determines weather or not a property will be used as a sortable field for search
		
		[FeaturedID]			    INT NOT NULL DEFAULT 0, -- Used to determin importance of a property so it can be featured on listing/details more promenantly (and in order of importance) 0 == not ordered in any prominant way
		--[Highlighted]				BIT NOT NULL DEFAULT 0, -- Determines weather or not a property will be used as part of it's listing/search results

		[OrderID]					INT NOT NULL DEFAULT 0,
		[FacetOrderID]				INT NOT NULL DEFAULT 0, -- As an alternative to OrderID, used to order facets (Not implemented).
		
		[Listing]					BIT NOT NULL DEFAULT 0, -- Property will be used in listing API results
		--[ListingOrderID]			INT NOT NULL DEFAULT 0, -- As an alternative to OrderID, used to order listing properties.
		
		[Details]					BIT NOT NULL DEFAULT 1, -- Property will be used in detail API results.
		--[DetailsOrderID]			INT NOT NULL DEFAULT 0, -- As an alternative to OrderID, used to order detail properties.		

		[CreatedDate]				DATETIME NOT NULL DEFAULT GETDATE()
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_PropertyIDKeyIndex] ON [#schema#].[Property] ([PropertyID])
GO
CREATE INDEX [#schema#_Index_PropertyNameIndex] ON [#schema#].[Property] ([PropertyName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_PropertyNameKeyClusteredIndex] ON [#schema#].[Property] ([PropertyNameKey])
GO