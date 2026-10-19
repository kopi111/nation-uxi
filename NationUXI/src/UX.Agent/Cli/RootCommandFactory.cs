using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UX.Agent.Client;
using UX.Agent.Collectors;
using UX.Agent.Identity;
using UX.Agent.Runtime;
using UX.Agent.Storage;

namespace UX.Agent.Cli;

public static class RootCommandFactory
{
    public static RootCommand Create()
    {
        var root = new RootCommand("UX Agent - Monitoring Agent");

        var run = new Command("run", "Run the agent as a background worker");
        run.SetHandler(RunWorkerAsync);
        root.AddCommand(run);

        var status = new Command("status", "Display agent status");
        status.SetHandler(ShowStatusAsync);
        root.AddCommand(status);

        return root;
    }

    private static async Task RunWorkerAsync()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddOptions<ControlPlaneOptions>()
            .Bind(builder.Configuration.GetSection("ControlPlane"));

        builder.Services.AddSingleton<FileStore>();
        builder.Services.AddSingleton<IdentityStorage>();
        builder.Services.AddSingleton<StatusCache>();
        builder.Services.AddSingleton<AgentConfigApplier>();

        builder.Services.AddSingleton<IInventoryCollector, BasicInventoryCollector>();

        if (OperatingSystem.IsWindows())
            builder.Services.AddSingleton<INetworkHintsCollector, WindowsNetworkHintsCollector>();
        else
            builder.Services.AddSingleton<INetworkHintsCollector, LinuxNetworkHintsCollector>();

        builder.Services.AddHttpClient<ControlPlaneClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ControlPlaneOptions>>().Value;
            var baseUrl = opts.BaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.HttpTimeoutSeconds);
        });

        builder.Services.AddSingleton(sp =>
        {
            var ids = sp.GetRequiredService<IdentityStorage>();
            return ids.LoadOrCreateAsync().GetAwaiter().GetResult();
        });

        builder.Services.AddHostedService<AgentWorker>();

        var host = builder.Build();
        await host.RunAsync();
    }

    private static async Task ShowStatusAsync()
    {
        var store = new FileStore(NullLogger<FileStore>.Instance);
        var status = await store.ReadAsync<AgentStatus>(Paths.GetStatusFilePath());

        if (status == null)
        {
            Console.WriteLine("No status available. Run the agent at least once.");
            return;
        }

        Console.WriteLine("UX Agent Status");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Agent ID:       {status.AgentId}");
        Console.WriteLine($"Machine Name:   {status.MachineName}");
        Console.WriteLine($"Last Check-in:  {status.LastCheckinUtc?.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Can Send:       {status.CanSend}");
        Console.WriteLine();
        Console.WriteLine("Config:");
        Console.WriteLine($"  Version:       {status.CurrentConfigVersion}");
        if (status.CurrentConfig != null)
        {
            Console.WriteLine($"  Pull Interval: {status.CurrentConfig.PullConfigEverySeconds}s");
            Console.WriteLine($"  Inventory:     {(status.CurrentConfig.EnableInventory ? "Enabled" : "Disabled")}");
            Console.WriteLine($"  Ingest URL:    {status.CurrentConfig.Ingest.Url}");
            Console.WriteLine($"  WiFi Policy:   {status.CurrentConfig.WifiPolicy.Mode}");
        }

        Console.WriteLine();
        if (status.LastTelemetry != null)
        {
            Console.WriteLine("Last Telemetry:");
            Console.WriteLine($"  Time:          {status.LastTelemetryUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Metrics:       {status.LastTelemetry.Metrics.Count}");
            Console.WriteLine($"  Inventory:     {status.LastTelemetry.Inventory.Count}");
        }
        else
        {
            Console.WriteLine("No telemetry data available");
        }
    }
}
