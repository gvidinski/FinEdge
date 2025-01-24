namespace FinEdgeAnalytics.Data
{
	public record EFData(Guid TransactionId, DateTime Date, float Amount, string Description);
}
