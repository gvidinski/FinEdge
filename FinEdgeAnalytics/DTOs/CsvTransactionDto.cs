namespace FinEdgeAnalytics.DTOs;

public record CsvTransactionDto(Guid TransactionId, string Date, decimal Amount, string Description);