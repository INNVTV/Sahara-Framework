CREATE TABLE [dbo].[PlatformUserRoles] (

    [Id]		NVARCHAR(128)		NOT NULL,
    [Name]		NVARCHAR(256)		NOT NULL,
    	
	CONSTRAINT [PK_dbo.PlatformUserRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[PlatformUserRoles]([Name] ASC)
GO