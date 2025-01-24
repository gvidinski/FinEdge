using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors
{
	public interface IEFExtracor : IExtractor
	{
		IAsyncEnumerable<List<EFTransactionDto>> GetTransactions();
	}
}
