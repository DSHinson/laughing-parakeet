using StateContainer.services;
using StateContainer.web.State;
using StateContainer.web.State.DataSource;

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
if (true)
{
    builder.Services.AddHostedService<TestDataEventSource>();
}
else {
   builder.Services.AddHostedService<PollingService>();
   builder.Services.AddHostedService<SaveToDbService>();
}

// Check for regular services
var regularServicesCount = builder.Services.Count(sd => sd.ServiceType == typeof(IDataSource));

// Check for hosted services
var hostedServicesCount = builder.Services
    .Where(sd => sd.ServiceType == typeof(IHostedService))
    .Select(sd => sd.ImplementationFactory?.Method?.ReturnType)
    .Concat(builder.Services.Where(sd => sd.ServiceType == typeof(IHostedService))
                    .Select(sd => sd.ImplementationType))
    .Count(type => type != null && typeof(IDataSource).IsAssignableFrom(type));

// Total count of IDataSource implementations
var totalCount = regularServicesCount + hostedServicesCount;

if (totalCount > 1)
{
    throw new Exception("Cannot have more than one instance of IDataSource in the service collection.");
}

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
