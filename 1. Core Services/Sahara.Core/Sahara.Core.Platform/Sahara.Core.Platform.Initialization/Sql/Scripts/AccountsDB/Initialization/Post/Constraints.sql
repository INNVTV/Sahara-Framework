ALTER TABLE Accounts
ADD FOREIGN KEY (PaymentPlanName)
REFERENCES PaymentPlans (PaymentPlanName)
GO

ALTER TABLE Accounts
ADD FOREIGN KEY (PaymentFrequencyMonths)
REFERENCES PaymentFrequencies (PaymentFrequencyMonths)
GO


--ALTER TABLE FailedPaymentAttempts
--ADD FOREIGN KEY (AccountID)
--REFERENCES Accounts (AccountID)
--GO




