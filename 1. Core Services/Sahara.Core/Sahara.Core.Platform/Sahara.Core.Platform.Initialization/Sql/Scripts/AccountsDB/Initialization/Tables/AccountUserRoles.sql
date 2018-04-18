CREATE TABLE [dbo].[AccountUserRoles] (

    [Id]		NVARCHAR(128)		NOT NULL,
    [Name]		NVARCHAR(256)		NOT NULL,

	CONSTRAINT [PK_dbo.AccountUserRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AccountUserRoles]([Name] ASC)
GO