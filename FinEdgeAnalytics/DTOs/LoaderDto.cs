namespace FinEdgeAnalytics.DTOs;

public record LoaderDto(Guid TransactionId, DateTime Date, decimal Amount, string Description);