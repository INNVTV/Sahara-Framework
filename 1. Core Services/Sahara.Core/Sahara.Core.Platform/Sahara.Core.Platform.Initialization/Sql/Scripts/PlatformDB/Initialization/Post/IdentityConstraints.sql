ALTER TABLE PlatformUserClaims
ADD CONSTRAINT [FK_dbo.PlatformUserClaims_dbo.PlatformUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[PlatformUsers] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE PlatformUserLogins
ADD CONSTRAINT [FK_dbo.PlatformUserLogins_dbo.PlatformUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[PlatformUsers] ([Id]) ON DELETE CASCADE
GO

ALTER TABLE PlatformUsersInRoles
ADD CONSTRAINT [FK_dbo.PlatformUsersInRoles_dbo.PlatformUserRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[PlatformUserRoles] ([Id]) ON DELETE CASCADE,
CONSTRAINT [FK_dbo.PlatformUsersInRoles_dbo.PlatformUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[PlatformUsers] ([Id]) ON DELETE CASCADE
GO