/* Placeholder */
CREATE TABLE [dbo].[StoragePartitions] (

    [Name]				NVARCHAR (100)		COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL PRIMARY KEY NONCLUSTERED, -- 'Free_1001', 'Shared_1001' or 'Dedicated_1001'
	[Key]				NVARCHAR (140)		NOT NULL,
	[URL]				NVARCHAR (100)		NOT NULL,
	[CDN]				NVARCHAR (100)		NOT NULL,
	[TenantCount]		INT					NOT NULL Default '0'

	--[StoragePartitionTierID]	NVARCHAR (80)		NOT NULL, 
	--[SequenceID]				INT					NOT NULL, 

	--[CreateDate]				DATETIME			NOT NULL,

	--[LastUpdatedDate]           DATETIME			NOT NULL,
)
GO

--ID is constraied to be unique & clustered
CREATE UNIQUE CLUSTERED INDEX [KeyIndex] ON [dbo].[StoragePartitions] ([Name])
GO



