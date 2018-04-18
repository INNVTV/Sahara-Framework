-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[ImageGroup](

		[ImageGroupTypeNameKey]		NVARCHAR (120) NOT NULL,

		[ImageGroupID]				[uniqueidentifier] NOT NULL,
		[ImageGroupName]			NVARCHAR (120) NOT NULL,
		[ImageGroupNameKey]			NVARCHAR (120) NOT NULL,

		[AllowDeletion]				BIT NOT NULL DEFAULT 1, --Blocks clients from deleting default groups, also used for counts of custom/default groups

		[OrderID]					INT NOT NULL DEFAULT 0,
		[Visible]					BIT NOT NULL DEFAULT 1,
		[CreatedDate]				DATETIME NOT NULL DEFAULT GETDATE()
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_ImageGroupIDKeyIndex] ON [#schema#].[ImageGroup] ([ImageGroupID])
GO
CREATE INDEX [#schema#_Index_ImageGroupNameIndex] ON [#schema#].[ImageGroup] ([ImageGroupName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_ImageGroupNameKeyClusteredIndex] ON [#schema#].[ImageGroup] ([ImageGroupNameKey])
GO