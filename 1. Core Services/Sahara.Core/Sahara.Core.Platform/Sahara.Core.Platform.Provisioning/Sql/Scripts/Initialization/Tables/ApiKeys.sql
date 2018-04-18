-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

CREATE TABLE [#schema#].[ApiKeys](
		[ApiKey]				UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
		[Name]					NVARCHAR (120) NOT NULL,
		[Description]			NVARCHAR (400) NOT NULL DEFAULT '',
		[CreatedDate]			DATETIME NOT NULL
	)
Go

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_ApiKeysIndex] ON [#schema#].[ApiKeys] ([ApiKey])
GO