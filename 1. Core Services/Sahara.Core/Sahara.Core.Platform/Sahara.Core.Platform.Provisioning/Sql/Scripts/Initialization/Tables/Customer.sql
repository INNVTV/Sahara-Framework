-- All instances of '#schema#' will be replaced by the AccountID of the account this schema is provisioned for

-- We keep the customer table very simple
-- use the [CustomerID] to cross refernce with addresses, purchases and other info on table storage for each account

CREATE TABLE [#schema#].[Customer](

		[CustomerID]			UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,

		[FirstName]				NVARCHAR(60) NOT NULL,
		[MiddleName]			NVARCHAR (60) NOT NULL,
		[LastName]				NVARCHAR (60) NOT NULL,
		[NickName]				NVARCHAR (60) NOT NULL,

		[Active]				BIT NOT NULL DEFAULT 1,
		[CreatedDate]			DATETIME NOT NULL
	)
Go

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [#schema#_Index_CustomerKeyIndex] ON [#schema#].[Customer] ([CustomerID])
GO
CREATE INDEX [#schema#_Index_CustomerLastNameIndex] ON [#schema#].[Customer] ([LastName])
GO
--CategoryLastName will be the hot query (alphabetical rolodex), can be used to search by routes so we CLUSTER our index on this: 
CREATE CLUSTERED INDEX [#schema#_Index_CustomerLastNameClusteredIndex] ON [#schema#].[Customer] ([LastName])
GO
