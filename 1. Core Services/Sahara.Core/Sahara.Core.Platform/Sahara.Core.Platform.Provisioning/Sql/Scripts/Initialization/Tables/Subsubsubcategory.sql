-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[Subsubsubcategory](

		--Foreign Key's and parent data updated by triggers	
		[SubsubcategoryID]				[uniqueidentifier] NOT NULL,

		[SubsubsubcategoryID]			UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
		[SubsubsubcategoryName]			NVARCHAR (120) NOT NULL,
		[SubsubsubcategoryNameKey]		NVARCHAR (120) NOT NULL,

		[Description]					NVARCHAR (1200) NOT NULL DEFAULT '',
		--[ShortDescription]			NVARCHAR (180) NOT NULL DEFAULT '',
		--[LongDescription]				NVARCHAR (580) NOT NULL DEFAULT '',

		[OrderID]						INT NOT NULL DEFAULT 0,
		[Visible]						BIT NOT NULL DEFAULT 1,

		[CreatedDate]					DATETIME NOT NULL
	)

GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_SubsubsubcategoryKeyIndex] ON [#schema#].[Subsubsubcategory] ([SubsubsubcategoryID])
GO
CREATE INDEX [#schema#_Index_SubsubsubcategoryNameIndex] ON [#schema#].[Subsubsubcategory] ([SubsubsubcategoryName])
GO
--SubcategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_SubsubsubcategoryNameKeyClusteredIndex] ON [#schema#].[Subsubsubcategory] ([SubsubsubcategoryNameKey])
GO