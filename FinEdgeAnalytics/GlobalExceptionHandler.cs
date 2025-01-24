using Microsoft.AspNetCore.Diagnostics;

namespace FinEdgeAnalytics;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILoggerFactory _loggerFactory;

	public GlobalExceptionHandler(ILoggerFactory loggerFactory)
	{
		_loggerFactory = loggerFactory;
	}

	public ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var logger = _loggerFactory.CreateLogger<GlobalExceptionHandler>();
		logger.LogError(exception, exception.Message);

		//httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
		//httpContext.Response.ContentType = "application/json";
		//var response = new
		//{
		//	Message = "Something went wrong. Please try again later.",
		//	Details = exception.Message

		//};

		//httpContext.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);

		// TODO: Better take that from settings
		// Return false to continue with the default behavior
		// - or - return true to signal that this exception is handled
		return ValueTask.FromResult(true);
	}
}