
using System.Reflection;

using FinEdgeAnalytics;
using FinEdgeAnalytics.Data;
using FinEdgeAnalytics.Loaders;

using IExtractor = FinEdgeAnalytics.Extractors.IExtractor;
using ITransformer = FinEdgeAnalytics.Transformers.ITransformer;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<EFDataContext>();

RegisterExtractors(builder);
RegisterTransformers(builder);
builder.Services.AddSingleton<FinEdgeLoader>();

var app = builder.Build();
app.UseExceptionHandler(o => { });

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<EFDataContext>();
	dbContext.SeedData();
}

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/run", async context =>
{
	var loader = app.Services.GetRequiredService<FinEdgeLoader>();
	var result = loader.ProcessData();

	await context.Response.WriteAsync(result);
});

app.Logger.LogInformation("Starting the application...");
app.Run();
return;

static void RegisterTransformers(WebApplicationBuilder builder)
{
	var assembly = Assembly.GetExecutingAssembly();
	var transformerTypes = assembly
		.GetTypes()
		.Where(t => t.GetInterfaces().Contains(typeof(ITransformer)) && t.IsClass && !t.IsAbstract);

	foreach (var type in transformerTypes)
	{
		builder.Services.AddTransient(typeof(ITransformer), type);
	}
}

static void RegisterExtractors(WebApplicationBuilder builder)
{
	var assembly = Assembly.GetExecutingAssembly();
	var extractorTypes = assembly
		.GetTypes()
		.Where(t => t.GetInterfaces().Contains(typeof(IExtractor)) && t.IsClass && !t.IsAbstract);

	foreach (var type in extractorTypes)
	{
		builder.Services.AddTransient(type, type);
	}
}