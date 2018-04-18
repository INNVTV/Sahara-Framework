-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[ImageFormat](

		--Parent Key's and parent data updated by triggers	
		[ImageGroupTypeNameKey]		NVARCHAR (120) NOT NULL,
		[ImageGroupNameKey]			NVARCHAR (120) NOT NULL,

		[ImageFormatID]				[uniqueidentifier] NOT NULL,
		[ImageFormatName]			NVARCHAR (120) NOT NULL,
		[ImageFormatNameKey]		NVARCHAR (120) NOT NULL,

		[Height]					INT NOT NULL DEFAULT 0,
		[Width]						INT NOT NULL DEFAULT 0,

		[Gallery]					BIT NOT NULL DEFAULT 0,
		[Listing]					BIT NOT NULL DEFAULT 0, --If true we merge this into search, category and other listing reults for products
		[Visible]					BIT NOT NULL DEFAULT 1,

		[AllowDeletion]				BIT NOT NULL DEFAULT 1, --Blocks clients from deleting default formats, also used for counts of custom/default groups

		[OrderID]					INT NOT NULL DEFAULT 0,

		[CreatedDate]				DATETIME NOT NULL DEFAULT GETDATE()
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_ImageFormatIDKeyIndex] ON [#schema#].[ImageFormat] ([ImageFormatID])
GO
CREATE INDEX [#schema#_Index_ImageFormatNameIndex] ON [#schema#].[ImageFormat] ([ImageFormatName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_ImageFormatNameKeyClusteredIndex] ON [#schema#].[ImageFormat] ([ImageFormatNameKey])
GO