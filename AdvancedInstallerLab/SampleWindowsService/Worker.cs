using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace SampleWindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string RegistryPath = @"SOFTWARE\AdvancedInstallerLab";
        private const string RegistryValueName = "GreetingMessage";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                string greeting = ReadGreetingFromRegistry() ?? "Hello from default service greeting!";
                _logger.LogInformation("Service heartbeat. Greeting: {Greeting}", greeting);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Service stopping.");
        }

        private string? ReadGreetingFromRegistry()
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryPath);
            if (key == null)
            {
                _logger.LogWarning("Registry key HKLM\\{Path} not found.", RegistryPath);
                return null;
            }

            object? value = key.GetValue(RegistryValueName);
            if (value is string s)
            {
                return s;
            }

            _logger.LogWarning("Registry value {Name} not found or not a string.", RegistryValueName);
            return null;
        }
    }
}
