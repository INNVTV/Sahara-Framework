-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for
--This table is used to track schema versions and upgrades

CREATE TABLE [#schema#].[SchemaLog](
	[Version] [decimal](5,1) NOT NULL PRIMARY KEY NONCLUSTERED,
	[Description]	[nvarchar](120) NOT NULL,
	[InstallDate] [datetime] NOT NULL DEFAULT GETUTCDATE(),
)
Go

CREATE CLUSTERED INDEX [#schema#_Index_SchemaLogClusteredIndex] ON [#schema#].[SchemaLog] ([Version])
Go
