CREATE TABLE [dbo].[AccountUserClaims] (

	[Id]			INT IDENTITY (1, 1)		NOT NULL,
	[UserId]		NVARCHAR(128)			NOT NULL,
    [ClaimType]		NVARCHAR (max),
    [ClaimValue]	NVARCHAR (max),

	CONSTRAINT [PK_dbo.AccountUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AccountUserClaims]([UserId] ASC)
GO
