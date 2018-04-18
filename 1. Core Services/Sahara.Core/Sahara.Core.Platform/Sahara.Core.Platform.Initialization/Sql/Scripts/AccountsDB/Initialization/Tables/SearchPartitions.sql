/* Placeholder */

CREATE TABLE [SearchPartitions] (

    [Name]						NVARCHAR (100)		COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL PRIMARY KEY NONCLUSTERED, -- 'Free_1001', 'Shared_1001' or 'Dedicated_1001'
	[Key]						NVARCHAR (160)		NOT NULL,
	[Plan]						NVARCHAR (40)		NOT NULL,             --  <--Free, Basic or Standard
	[TenantCount]				INT					NOT NULL Default '0'  --  <--Max: Free (3=500 docs per account), Basic (5=200,000 documents per account), Standard (15=documents per 1,000,000)

	--[SearchPartitionTierID]	NVARCHAR (80)		NOT NULL, 
	--[SequenceID]				INT					NOT NULL, 
	
	--[CreateDate]				DATETIME			NOT NULL,
	--[Name]					NVARCHAR (100)		NOT NULL,

	--[LastUpdatedDate]         DATETIME			NOT NULL,
)
GO

--ID is constraied to be unique & clustered
CREATE UNIQUE CLUSTERED INDEX [KeyIndex] ON [dbo].[SearchPartitions] ([Name])
GO



-- Names: 'sahara-stage-standard-1001'
