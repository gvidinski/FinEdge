using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors;

public class ApiExtractor : IApiExtractor
{
	private readonly ILogger<ApiExtractor> _logger;
	private readonly string _apiUrl;

	public ApiExtractor(
		ILogger<ApiExtractor> logger,
		IConfiguration config)
	{
		_logger = logger;
		_apiUrl = config.GetSection("ApiExtractor:ApiUrl").Exists()
			? config.GetValue<string>("ApiExtractor:ApiUrl")
			: string.Empty;

		CheckApiUrl();
	}


	public async IAsyncEnumerable<List<CsvTransactionDto>> GetTransactions()
	{
		var transactions = new List<CsvTransactionDto>();

		// Call the API and populate the transactions list

		await Task.Yield();

		yield return transactions;
	}

	private void CheckApiUrl()
	{
		if (string.IsNullOrWhiteSpace(_apiUrl))
		{
			throw new Exception(message: "ApiUrl is not set in the configuration file.");
		}
	}
}