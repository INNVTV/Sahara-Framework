-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[Category](

	[CategoryID]			UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[CategoryName]			NVARCHAR(120) NOT NULL,
	[CategoryNameKey]		NVARCHAR (120) NOT NULL,

	[Description]			NVARCHAR (1200) NOT NULL DEFAULT '',
	--[ShortDescription]	NVARCHAR (180) NOT NULL DEFAULT '',
	--[LongDescription]		NVARCHAR (580) NOT NULL DEFAULT '',

	[Visible]				BIT NOT NULL DEFAULT 1,
	[OrderID]				INT NOT NULL DEFAULT 0,

	[CreatedDate]			DATETIME NOT NULL

	)
Go

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_CategoryKeyIndex] ON [#schema#].[Category] ([CategoryID])
GO
CREATE INDEX [#schema#_Index_CategoryNameIndex] ON [#schema#].[Category] ([CategoryName])
GO
--CategoryNameKey will be the hot query, can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_CategoryNameKeyClusteredIndex] ON [#schema#].[Category] ([CategoryNameKey])
GO
