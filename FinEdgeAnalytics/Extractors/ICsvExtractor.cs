using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors
{
	public interface ICsvExtractor : IExtractor
	{
		IAsyncEnumerable<List<CsvTransactionDto>> GetTransactions();
	}
}
