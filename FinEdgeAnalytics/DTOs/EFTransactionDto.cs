namespace FinEdgeAnalytics.DTOs
{
	public record EFTransactionDto(Guid TransactionId, DateTime Date, float Amount, string Description);
}
