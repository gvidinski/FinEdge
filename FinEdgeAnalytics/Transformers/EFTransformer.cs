using FinEdgeAnalytics.DTOs;
using FinEdgeAnalytics.Extractors;

namespace FinEdgeAnalytics.Transformers;

public class EFTransformer : ITransformer
{
	private readonly ILogger<EFTransformer> _logger;
	private readonly IServiceProvider _services;
	private readonly int _descriptionLength;

	private const int DefaultDescriptionLength = 100;
	private const string ServiceNotRegistered = "EFInMemoryExtractor is not registered in the services collection.";

	public EFTransformer(
		ILogger<EFTransformer> logger,
		IConfiguration config,
		IServiceProvider services)
	{
		_logger = logger;
		_services = services;
		_descriptionLength = config.GetSection("CsvTransformer:DescriptionLength").Exists()
			? config.GetValue<int>("CsvTransformer:DescriptionLength")
			: DefaultDescriptionLength;
	}

	public async IAsyncEnumerable<List<LoaderDto>> TransformData()
	{
		var efExtractor = _services.GetRequiredService<EFInMemoryExtractor>();
		if (efExtractor is null)
		{
			throw new Exception(message: ServiceNotRegistered);
		}

		var filters = new List<Func<LoaderDto, bool>>
		{
			item => item.Amount < (decimal)1000.00,
			item => item.Date >= DateTime.Parse("2025-01-01 00:00:00")
		};

		var enumerator = efExtractor.GetTransactions().GetAsyncEnumerator();
		while (await enumerator.MoveNextAsync())
		{
			var transactions = enumerator.Current;
			var transformed = ConvertData(transactions, _descriptionLength);
			var uniqueList = transformed.RemoveDuplicates().AsParallel();

			foreach (var filter in filters)
			{
				uniqueList = uniqueList.Where(filter);
			}

			var result = uniqueList.ToList();
			_logger.LogInformation($"Transforming {result.Count} {Helpers.Pluralize("transaction", result.Count)}...");

			yield return result;
		}
	}

	public static List<LoaderDto> ConvertData(List<EFTransactionDto> transactions, int descriptionLength)
	{
		var result = transactions
			.AsParallel()
			.Select(transaction => new LoaderDto(
				transaction.TransactionId,
				transaction.Date.ToUniversalTime(),
				(decimal)transaction.Amount,
				transaction.Description.Length > descriptionLength
					? transaction.Description.Substring(0, descriptionLength) // or we can use range indexer transaction.Description[..descriptionLength]
					: transaction.Description
			))
			.ToList();

		return result;
	}
}