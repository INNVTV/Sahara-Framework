CREATE TABLE [dbo].[AccountUserLogins] (

    [UserId]			NVARCHAR(128)			NOT NULL,
    [LoginProvider]		NVARCHAR(128)			NOT NULL,
    [ProviderKey]		NVARCHAR(128)			NOT NULL,
    
    CONSTRAINT [PK_dbo.AccountUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AccountUserLogins]([UserId] ASC)
GO