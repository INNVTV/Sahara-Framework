-- Only for certain scenarios

/*CREATE TABLE [dbo].[AccountUsersInAccounts] (

    [AccountId]				[nvarchar](128)			NOT NULL,
    [UserId]				[nvarchar](128)			NOT NULL,
	--[Role]					[nvarchar](128)			NOT NULL,
	[Owner]					BIT						NOT NULL	DEFAULT 0, -- Is an account owner, can delete the account and transfer ownership


    CONSTRAINT [PK_dbo.AccountUsersInAccounts] PRIMARY KEY ([AccountId], [UserId])
)*/