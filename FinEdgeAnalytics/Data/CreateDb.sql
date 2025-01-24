IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'FinEdge')
BEGIN
    CREATE DATABASE [FinEdge];
END;
GO

USE [FinEdge];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Transactions](
        [TransactionId] uniqueidentifier NOT NULL,
        [TransactionDate] datetime NOT NULL,
        [Amount] decimal(18, 2) NOT NULL,
        [Description] nchar(255) NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([TransactionId] ASC)
    );

	INSERT INTO [dbo].[Transactions] ([TransactionId], [TransactionDate], [Amount], [Description])
        VALUES 
		(NEWID(), '2024-12-02 12:00:00', 150.00, 'Transaction 1'),
		(NEWID(), '2025-12-22 13:30:15', 200.00, 'Transaction 2'),
		(NEWID(), '2025-01-05 14:45:30', 300.00, 'Transaction 3'),
		(NEWID(), '2025-01-15 16:10:45', 400.00, 'Transaction 4'),
		(NEWID(), '2025-01-22 17:55:59', 500.00, 'Transaction 5');
END;
GO
