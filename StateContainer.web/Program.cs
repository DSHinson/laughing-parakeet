using Docker.DotNet.Models;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StateContainer.services;
using StateContainer.web.Docker;
using StateContainer.web.Logging;
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
        builder.Logging.AddOpenTelemetry(logging => logging.AddFileExporter($"logs/{DateTime.Now.ToString("yyyyMMdd")}logs_output.txt"));

        ApiKeyProvider apiKeyProvider = new ApiKeyProvider();
        string apiKey = apiKeyProvider.GetApiKey();

        builder.Services.AddTransient(sp =>
        {
            return new WorldCoinService(apiKey, sp.GetRequiredService<HttpClient>());
        });

        builder.Services.AddSingleton<MarketStateContainer>();
        builder.Services.AddScoped<jsLoggingService>();
        // Register the concrete implementation
        builder.Services.AddScoped<CounterService>();
        builder.Services.AddScoped<SaveToDbService>();

        builder.Services.AddScoped<ICounterService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CounterService>>();
            var service = provider.GetRequiredService<CounterService>();
            return ServiceLoggingProxy<ICounterService>.Create(service, logger);
        });

        if (false)
        {
            builder.Services.AddHostedService<TestDataEventSource>();
        }
        else
        {
            builder.Services.AddHostedService<PollingService>();
            builder.Services.AddScoped<ISaveToDbService>(provider =>
            {
                var Service = provider.GetRequiredService<SaveToDbService>();
                var logger = provider.GetRequiredService<ILogger<SaveToDbService>>();
                return ServiceLoggingProxy<SaveToDbService>.Create(Service, logger);
            });
        }
        //For more info: https://www.milanjovanovic.tech/blog/introduction-to-distributed-tracing-with-opentelemetry-in-dotnet
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("StateContainer"))
            .WithMetrics(metrics => {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                // Use custom file exporter
                metrics.AddFileExporter($"logs/{DateTime.Now.ToString("yyyyMMdd")}metrics_output.txt");
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                // Use custom file exporter
                tracing.AddFileExporter($"logs/{DateTime.Now.ToString("yyyyMMdd")}traces_output.txt");
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
                Console.WriteLine("Docker Output:");
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

    private static async void StartDocker()
    {
        DockerConfig dockerConfig = new DockerConfig()
        {
            ImageName = "mcr.microsoft.com/dotnet/nightly/aspire-dashboard",
            Tag = "8.1",
            ContainerName = "my-aspire-dashboard-container",
            HostPort = 18888,
            ContainerPort = 18888,
        };

        DockerService dockerService = new DockerService();
        await dockerService.PullImageAsync(dockerConfig.ImageName, dockerConfig.Tag);
        string containerId = await dockerService.CreateContainerAsync(dockerConfig);

        bool started = await dockerService.StartContainerAsync(containerId);

        if (started)
        {
            Console.WriteLine($"Container started with ID: {containerId}");
            await dockerService.CaptureLogAsync(containerId);
        }
        else
        {
            Console.WriteLine("Failed to start the container.");
        }
    }
}