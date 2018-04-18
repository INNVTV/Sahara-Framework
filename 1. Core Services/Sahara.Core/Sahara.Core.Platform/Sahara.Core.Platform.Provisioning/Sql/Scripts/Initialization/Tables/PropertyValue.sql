-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[PropertyValue](

		--Foreign Key's and parent data updated by triggers	
		[PropertyValueID]			UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
		[PropertyValueName]			NVARCHAR (120) NOT NULL,
        [PropertyValueNameKey]		NVARCHAR (120) NOT NULL,

		[PropertyID]				[uniqueidentifier] NOT NULL,

		[OrderID]					INT NOT NULL DEFAULT 0,
		[Visible]					BIT NOT NULL DEFAULT 1,
		[CreatedDate]				DATETIME NOT NULL
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_PropertyValueIDKeyIndex] ON [#schema#].[PropertyValue] ([PropertyValueID])
GO
CREATE INDEX [#schema#_Index_PropertyValueNameIndex] ON [#schema#].[PropertyValue] ([PropertyValueName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_PropertyValueNameKeyClusteredIndex] ON [#schema#].[PropertyValue] ([PropertyValueNameKey])
GO