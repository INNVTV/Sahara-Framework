ALTER TABLE AccountUserClaims
ADD CONSTRAINT [FK_dbo.AccountUserClaims_dbo.AccountUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccountUsers] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE AccountUserLogins
ADD CONSTRAINT [FK_dbo.AccountUserLogins_dbo.AccountUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccountUsers] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE AccountUsersInRoles
ADD CONSTRAINT [FK_dbo.AccountUsersInRoles_dbo.AccountUserRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AccountUserRoles] ([Id]) ON DELETE CASCADE,
CONSTRAINT [FK_dbo.AccountUsersInRoles_dbo.AccountUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccountUsers] ([Id]) ON DELETE CASCADE
GO