using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors
{
	public interface IApiExtractor : IExtractor
	{
		IAsyncEnumerable<List<CsvTransactionDto>> GetTransactions();
	}
}
