
CREATE TABLE [PropertyType](

		--Foreign Key's and parent data updated by triggers	
		[PropertyTypeID]				[uniqueidentifier] NOT NULL,

		[PropertyTypeName]				NVARCHAR (120) NOT NULL,
		[PropertyTypeNameKey]			NVARCHAR (120) NOT NULL,

		[PropertyTypeDescription]		NVARCHAR (320) NOT NULL,

		[OrderID]						INT NOT NULL DEFAULT 0,
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [Index_PropertyTypeIDKeyIndex] ON [PropertyType] ([PropertyTypeID])
GO
CREATE INDEX [Index_PropertyTypeNameIndex] ON [PropertyType] ([PropertyTypeName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [Index_PropertyTypeNameKeyClusteredIndex] ON [PropertyType] ([PropertyTypeNameKey])
GO