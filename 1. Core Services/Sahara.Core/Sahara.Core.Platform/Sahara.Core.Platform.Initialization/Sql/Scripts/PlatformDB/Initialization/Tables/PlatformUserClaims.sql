CREATE TABLE [dbo].[PlatformUserClaims] (

    [Id]			INT IDENTITY (1, 1)		NOT NULL,
	[UserId]		NVARCHAR(128)			NOT NULL,
    [ClaimType]		NVARCHAR(max),
    [ClaimValue]	NVARCHAR(max),

	CONSTRAINT [PK_dbo.PlatformUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[PlatformUserClaims]([UserId] ASC)
GO
