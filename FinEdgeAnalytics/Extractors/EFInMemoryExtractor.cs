using FinEdgeAnalytics.Data;
using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors
{
	public class EFInMemoryExtractor : IEFExtracor
	{
		private readonly ILogger<EFInMemoryExtractor> _logger;

		public EFInMemoryExtractor(ILogger<EFInMemoryExtractor> logger)
		{
			_logger = logger;
		}

		public async IAsyncEnumerable<List<EFTransactionDto>> GetTransactions()
		{
			await using var context = new EFDataContext();

			await context.Database.EnsureCreatedAsync();

			var transaction = context.IncomingData
				.Select(record => new EFTransactionDto(
					record.TransactionId,
					record.Date,
					record.Amount,
					record.Description))
				.ToList();

			yield return transaction;
		}
	}
}
