USE [FinEdge]
GO

IF NOT EXISTS (SELECT * FROM sys.table_types WHERE name = 'TransactionType')
BEGIN
    CREATE TYPE [dbo].[TransactionType] AS TABLE
    (
        TransactionId uniqueidentifier,
        TransactionDate datetime,
        Amount decimal(18, 2),
        Description nvarchar(255)
    );
END;
GO

DROP PROCEDURE IF EXISTS [dbo].[InsertOrUpdateTransactions];
GO

CREATE PROCEDURE [dbo].[InsertOrUpdateTransactions]
    @Transactions dbo.TransactionType READONLY,
    @InsertedCount INT OUTPUT,
    @UpdatedCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Inserted INT;
        DECLARE @Updated INT;

        DECLARE @MergeOutput TABLE (Action VARCHAR(10));

        MERGE Transactions AS target
        USING @Transactions AS source
        ON target.TransactionId = source.TransactionId
        WHEN MATCHED THEN
            UPDATE SET 
                target.TransactionDate = source.TransactionDate,
                target.Amount = source.Amount,
                target.Description = source.Description
        WHEN NOT MATCHED THEN
            INSERT (TransactionId, TransactionDate, Amount, Description)
            VALUES (source.TransactionId, source.TransactionDate, source.Amount, source.Description)
        OUTPUT $action INTO @MergeOutput (Action);

        SET @Inserted = (SELECT COUNT(*) FROM @MergeOutput WHERE Action = 'INSERT');
        SET @Updated = (SELECT COUNT(*) FROM @MergeOutput WHERE Action = 'UPDATE');

        SET @InsertedCount = @Inserted;
        SET @UpdatedCount = @Updated;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        -- Handle errors
        DECLARE @ErrorMessage nvarchar(4000);
        DECLARE @ErrorSeverity int;
        DECLARE @ErrorState int;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END





