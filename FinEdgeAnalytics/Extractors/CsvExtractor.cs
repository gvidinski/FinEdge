using System.Globalization;

using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Extractors;

public class CsvExtractor : ICsvExtractor
{
	private readonly ILogger<CsvExtractor> _logger;
	private readonly string _csvFilePath;
	private readonly int _csvBatchSize;
	private readonly string _csvSeparator;
	private const string CsvFilePathNotSetMessage = "CsvFilePath is not set in the configuration file.";

	public CsvExtractor(
		ILogger<CsvExtractor> logger,
		IConfiguration config)
	{
		_logger = logger;
		_csvFilePath = config.GetValue<string>("CsvExtractor:CsvFilePath");
		_csvBatchSize = config.GetSection("CsvExtractor:CsvBatchSize").Exists() ? config.GetValue<int>("CsvExtractor:CsvBatchSize") : 10;
		_csvSeparator = config.GetSection("CsvExtractor:CsvSeparator").Exists() ? config.GetValue<string>("CsvExtractor:CsvSeparator") : ",";
	}

	public async IAsyncEnumerable<List<CsvTransactionDto>> GetTransactions()
	{
		if (string.IsNullOrWhiteSpace(_csvFilePath))
		{
			throw new Exception(message: CsvFilePathNotSetMessage);
		}

		var currentRow = 0;
		var transactions = new List<CsvTransactionDto>();

		using (var reader = new StreamReader(_csvFilePath))
		{
			string line;
			string[] headers = null;

			while ((line = await reader.ReadLineAsync()) != null)
			{
				currentRow++;
				var values = line.Split(_csvSeparator);

				if (headers is null)
				{
					headers = values;
				}
				else
				{
					var transaction = GetCsvTransactionDto(values, headers);
					transactions.Add(transaction);
				}

				if (currentRow != _csvBatchSize) continue;

				LogExtractionProcess(transactions.Count);
				yield return new List<CsvTransactionDto>(transactions);

				transactions.Clear();
				currentRow = 0;
			}
		}

		if (transactions.Count <= 0) yield break;

		LogExtractionProcess(transactions.Count, true);
		yield return transactions;
	}

	private static CsvTransactionDto GetCsvTransactionDto(string[] values, string[] headers)
	{
		var transaction = new CsvTransactionDto(
			Guid.Parse(values[Array.IndexOf(headers, "TransactionID")]),
			values[Array.IndexOf(headers, "Date")],
			decimal.Parse(values[Array.IndexOf(headers, "Amount")], CultureInfo.InvariantCulture),
			values[Array.IndexOf(headers, "Description")]
		);

		return transaction;
	}

	private void LogExtractionProcess(int count, bool isFinal = false)
	{
		_logger.LogInformation($"Extracted {count} {(isFinal ? "final " : "")}{Helpers.Pluralize("transaction", count)} from CSV \"{_csvFilePath}\".");
	}
}