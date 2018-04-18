CREATE TABLE [AccountUsersInRoles] (

    [UserId] NVARCHAR(128) NOT NULL,
    [RoleId] NVARCHAR(128) NOT NULL,

    CONSTRAINT [PK_dbo.AccountUsersInRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
)
GO

CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AccountUsersInRoles]([UserId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[AccountUsersInRoles]([RoleId] ASC)
GO