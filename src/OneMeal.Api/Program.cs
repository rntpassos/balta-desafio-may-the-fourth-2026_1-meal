using OneMeal.Ai;
using OneMeal.Application;
using OneMeal.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
	options.AddPolicy("frontend", policy =>
	{
		policy.WithOrigins("https://localhost:7274", "http://localhost:5274")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
builder.Services
	.AddApplication()
	.AddInfrastructure()
	.AddAi();

var app = builder.Build();

var hasHttpsBinding = app.Urls.Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (hasHttpsBinding)
{
	app.UseHttpsRedirection();
}

app.UseCors("frontend");
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", mode = "deterministic-fallback" }));

app.Run();
