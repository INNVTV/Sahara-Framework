-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[PropertySwatch](

		--Foreign Key's and parent data updated by triggers	
		[PropertySwatchID]				UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
		[PropertySwatchImage]			NVARCHAR (240) NOT NULL,
		[PropertySwatchImageMedium]		NVARCHAR (240) NOT NULL,
		[PropertySwatchImageSmall]		NVARCHAR (240) NOT NULL,
		[PropertySwatchLabel]			NVARCHAR (120) NOT NULL,
        [PropertySwatchNameKey]			NVARCHAR (120) NOT NULL,

		[PropertyID]					[uniqueidentifier] NOT NULL,

		[OrderID]						INT NOT NULL DEFAULT 0,
		[Visible]						BIT NOT NULL DEFAULT 1,
		[CreatedDate]					DATETIME NOT NULL
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_PropertySwatchIDKeyIndex] ON [#schema#].[PropertySwatch] ([PropertySwatchID])
GO
CREATE INDEX [#schema#_Index_PropertySwatchLabelIndex] ON [#schema#].[PropertySwatch] ([PropertySwatchLabel])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_PropertySwatchNameKeyClusteredIndex] ON [#schema#].[PropertySwatch] ([PropertySwatchNameKey])
GO