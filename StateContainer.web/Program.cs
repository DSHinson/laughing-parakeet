using StateContainer.services;
using StateContainer.web.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Register HttpClient for dependency injection
builder.Services.AddHttpClient();

ApiKeyProvider apiKeyProvider = new ApiKeyProvider();
string apiKey = apiKeyProvider.GetApiKey();

builder.Services.AddTransient<WorldCoinService>(sp =>
{
    return new WorldCoinService(apiKey, sp.GetRequiredService<HttpClient>());
});

builder.Services.AddSingleton<MarketStateContainer>();

builder.Services.AddHostedService<TestDataEventSource>();
//builder.Services.AddHostedService<PollingService>();
//builder.Services.AddHostedService<SaveToDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
