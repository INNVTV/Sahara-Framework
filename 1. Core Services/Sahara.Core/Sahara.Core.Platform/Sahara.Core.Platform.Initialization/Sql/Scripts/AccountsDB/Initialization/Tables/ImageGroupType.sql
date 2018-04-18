CREATE TABLE [ImageGroupType](

		--Foreign Key's and parent data updated by triggers	
		[ImageGroupTypeID]				[uniqueidentifier] NOT NULL,

		[ImageGroupTypeName]			NVARCHAR (120) NOT NULL,
		[ImageGroupTypeNameKey]			NVARCHAR (120) NOT NULL,
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [Index_ImageGroupTypeIDKeyIndex] ON [ImageGroupType] ([ImageGroupTypeID])
GO
CREATE INDEX [Index_ImageGroupTypeNameIndex] ON [ImageGroupType] ([ImageGroupTypeName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [Index_ImageGroupTypeNameKeyClusteredIndex] ON [ImageGroupType] ([ImageGroupTypeNameKey])
GO