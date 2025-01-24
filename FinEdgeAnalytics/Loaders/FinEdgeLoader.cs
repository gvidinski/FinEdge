using System.Data;

using FinEdgeAnalytics.DTOs;
using FinEdgeAnalytics.Transformers;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;

namespace FinEdgeAnalytics.Loaders;

public class FinEdgeLoader : ILoader
{
	private readonly ILogger<FinEdgeLoader> _logger;
	private readonly IServiceProvider _services;
	private readonly IExceptionHandler _globalExceptionHandler;
	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly string _connectionString;
	private static bool _isTaskRunning = false;
	private static readonly object LockObject = new();
	private readonly CancellationTokenSource _cancellationTokenSource = new();

	public FinEdgeLoader(
		ILogger<FinEdgeLoader> logger,
		IConfiguration config,
		IServiceProvider services,
		IExceptionHandler globalExceptionHandler,
		IHttpContextAccessor httpContextAccessor)
	{
		_logger = logger;
		_services = services;
		_globalExceptionHandler = globalExceptionHandler;
		_httpContextAccessor = httpContextAccessor;
		_connectionString = config.GetConnectionString("DefaultConnection");
	}

	public string ProcessData()
	{
		lock (LockObject)
		{
			if (_isTaskRunning)
			{
				return "Process is already running.";
			}

			_isTaskRunning = true;
		}
		try
		{
			Task.Factory.StartNew(async () =>
			{
				try
				{
					await Runner();
				}
				catch (Exception ex)
				{
					var httpContext = _httpContextAccessor.HttpContext;
					await _globalExceptionHandler.TryHandleAsync(httpContext, ex, _cancellationTokenSource.Token);
				}
				finally
				{
					lock (LockObject)
					{
						_isTaskRunning = false;
					}
				}
			}, _cancellationTokenSource.Token, TaskCreationOptions.None, TaskScheduler.Default);
		}
		catch (Exception)
		{
			lock (LockObject)
			{
				_isTaskRunning = false;
			}
			throw;
		}

		return "Process started";
	}

	public async Task Runner()
	{
		var transformers = _services.GetServices<ITransformer>();
		foreach (var transformer in transformers)
		{
			var enumerator = transformer.TransformData().GetAsyncEnumerator();
			while (await enumerator.MoveNextAsync())
			{
				var data = enumerator.Current;
				await LoadData(data);
			}

			// Let's simulate some job delay
			await Task.Delay(5000);
		}

		_logger.LogInformation("Process completed");

		lock (LockObject)
		{
			_isTaskRunning = false;
		}
	}

	private async Task LoadData(List<LoaderDto> data)
	{
		_logger.LogInformation($"Loading {data.Count} {Helpers.Pluralize("transaction", data.Count)}...");

		var transactionsTable = ConvertToDataTable(data);

		await using var connection = new SqlConnection(_connectionString);
		await using var command = new SqlCommand("InsertOrUpdateTransactions", connection);

		command.CommandType = CommandType.StoredProcedure;

		var parameter = new SqlParameter
		{
			ParameterName = "@Transactions",
			SqlDbType = SqlDbType.Structured,
			TypeName = "dbo.TransactionType",
			Value = transactionsTable
		};
		command.Parameters.Add(parameter);

		var insertedParam = new SqlParameter("@InsertedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
		command.Parameters.Add(insertedParam);

		var updatedParam = new SqlParameter("@UpdatedCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
		command.Parameters.Add(updatedParam);

		connection.Open();
		await command.ExecuteNonQueryAsync();

		var insertedCount = insertedParam.Value is not null ? (int)insertedParam.Value : -1;
		var updatedCount = updatedParam.Value is not null ? (int)updatedParam.Value : -1;

		_logger.LogInformation($"Rows Inserted: {insertedCount}");
		_logger.LogInformation($"Rows Updated: {updatedCount}");

		connection.Close();
	}

	public DataTable ConvertToDataTable(List<LoaderDto> transactions)
	{
		var table = new DataTable();
		table.Columns.Add("TransactionId", typeof(Guid));
		table.Columns.Add("TransactionDate", typeof(DateTime));
		table.Columns.Add("Amount", typeof(decimal));
		table.Columns.Add("Description", typeof(string));

		foreach (var transaction in transactions)
		{
			table.Rows.Add(transaction.TransactionId, transaction.Date, transaction.Amount, transaction.Description);
		}

		return table;
	}

}