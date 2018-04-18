CREATE TABLE [dbo].[PlatformUserLogins] (

    [UserId]			NVARCHAR(128)		NOT NULL,
    [LoginProvider]		NVARCHAR(128)		NOT NULL,
    [ProviderKey]		NVARCHAR(128)		NOT NULL,

    CONSTRAINT [PK_dbo.PlatformUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[PlatformUserLogins]([UserId] ASC)
GO
