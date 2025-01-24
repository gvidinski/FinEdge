using FinEdgeAnalytics.DTOs;
using FinEdgeAnalytics.Extractors;

namespace FinEdgeAnalytics.Transformers;

public class ApiTransformer : ITransformer
{
	private readonly IServiceProvider _services;
	private readonly ILogger<ApiTransformer> _logger;

	public ApiTransformer(
		ILogger<ApiTransformer> logger,
		IConfiguration config, IServiceProvider services)
	{
		_logger = logger;
		_services = services;
	}
	public async IAsyncEnumerable<List<LoaderDto>> TransformData()
	{
		_logger.LogInformation("Transforming data from API...");
		var apiExtractor = _services.GetRequiredService<ApiExtractor>();
		if (apiExtractor is null)
		{
			throw new Exception(message: "ApiExtractor is not registered in the services collection.");
		}

		var filters = new List<Func<LoaderDto, bool>>
		{
			item => item.Amount < (decimal)1000.00,
			item => item.Date >= DateTime.Parse("2025-01-01 00:00:00")
		};

		var enumerator = apiExtractor.GetTransactions().GetAsyncEnumerator();
		while (await enumerator.MoveNextAsync())
		{
			var transactions = enumerator.Current;
			var transformed = new List<LoaderDto>();
			var uniqueList = transformed.RemoveDuplicates().AsParallel();

			foreach (var filter in filters)
			{
				uniqueList = uniqueList.Where(filter);
			}

			var result = uniqueList.ToList();
			_logger.LogInformation($"Transforming {result.Count} {Helpers.Pluralize("transaction", result.Count)}...");

			yield return [];
		}

	}
}