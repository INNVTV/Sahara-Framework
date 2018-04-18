-- Update AccountName/AccountNameKey throughtout schema when changed
CREATE TRIGGER AccountNameTrigger ON dbo.Accounts
AFTER UPDATE
AS

IF UPDATE(AccountName)
	UPDATE	users
	SET		users.AccountName = i.AccountName, users.AccountNameKey = i.AccountNameKey
	FROM	dbo.AccountUsers AS users, INSERTED as i
	WHERE	users.AccountID = i.AccountID
	GO
