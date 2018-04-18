CREATE TABLE [dbo].[PlatformUsersInRoles] (

    [UserId] NVARCHAR(128) NOT NULL,
    [RoleId] NVARCHAR(128) NOT NULL,

	CONSTRAINT [PK_dbo.PlatformUsersInRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[PlatformUsersInRoles]([UserId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[PlatformUsersInRoles]([RoleId] ASC)
GO