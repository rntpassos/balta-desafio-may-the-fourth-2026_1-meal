using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OneMeal.Web;
using OneMeal.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5181/";
if (!apiBaseUrl.EndsWith('/'))
{
	apiBaseUrl += "/";
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<MealSuggestionApiClient>();

await builder.Build().RunAsync();
