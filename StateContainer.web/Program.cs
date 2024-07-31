using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StateContainer.services;
using StateContainer.web.State;
using StateContainer.web.State.DataSource;
using System.Diagnostics;
using System.Diagnostics.Metrics;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        // Register HttpClient for dependency injection
        builder.Services.AddHttpClient();
        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

        ApiKeyProvider apiKeyProvider = new ApiKeyProvider();
        string apiKey = apiKeyProvider.GetApiKey();

        builder.Services.AddTransient(sp =>
        {
            return new WorldCoinService(apiKey, sp.GetRequiredService<HttpClient>());
        });

        builder.Services.AddSingleton<MarketStateContainer>();
        if (true)
        {
            builder.Services.AddHostedService<TestDataEventSource>();
        }
        else
        {
            builder.Services.AddHostedService<PollingService>();
            builder.Services.AddHostedService<SaveToDbService>();
        }
        //For more info: https://www.milanjovanovic.tech/blog/introduction-to-distributed-tracing-with-opentelemetry-in-dotnet
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("StateContainer"))
            .WithMetrics(metrics => {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                metrics.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:18888"));
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql();

                tracing.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:18888"));
            });
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
        //string command = "docker run -d -p 4317:4317 -p 16686:16686 jaegertracing/all-in-one:latest";
        //RunStartupCommand(command);
        string command = "docker run -d -p 18888:18888 mcr.microsoft.com/dotnet/nightly/aspire-dashboard:8.1";
        RunStartupCommand(command);
        app.Run();
    }

    private static void RunStartupCommand(string command)
    {
        

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {command}", // /C executes the command and then exits
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true // Run without creating a new window
        };

        try
        {
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Read output and error
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Log the output (if needed)
                Console.WriteLine("Output:");
                Console.WriteLine(output);
                Console.WriteLine("Error:");
                Console.WriteLine(error);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log the error)
            Console.WriteLine("An error occurred while running the command:");
            Console.WriteLine(ex.Message);
        }
    }
}