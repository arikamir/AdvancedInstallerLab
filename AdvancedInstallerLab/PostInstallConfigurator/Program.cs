using Microsoft.Win32;

namespace PostInstallConfigurator
{
    public class Program
    {
        // Convention: exit code 0 = success, non-zero = failure
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: PostInstallConfigurator <mode> [options]");
                    Console.WriteLine("Modes: install, uninstall");
                    return 1;
                }

                string mode = args[0].ToLowerInvariant();

                switch (mode)
                {
                    case "install":
                        return RunInstallActions(args.Skip(1).ToArray());
                    case "uninstall":
                        return RunUninstallActions(args.Skip(1).ToArray());
                    default:
                        Console.WriteLine($"Unknown mode: {mode}");
                        return 2;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fatal error: " + ex);
                return 99;
            }
        }

        private static int RunInstallActions(string[] args)
        {
            Console.WriteLine("Running post-install actions...");

            // Example: write a registry value that the service can also read
            const string registryPath = @"SOFTWARE\AdvancedInstallerLab";
            const string registryValueName = "GreetingMessage";

            string greeting = args.FirstOrDefault()
                              ?? "Hello from MSI custom action!";

            using RegistryKey key = Registry.LocalMachine.CreateSubKey(registryPath, true)
                ?? throw new InvalidOperationException($"Failed to create HKLM\\{registryPath}");

            key.SetValue(registryValueName, greeting, RegistryValueKind.String);

            // Example: create a marker file in ProgramData
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string dir = Path.Combine(programData, "AdvancedInstallerLab");
            Directory.CreateDirectory(dir);

            string filePath = Path.Combine(dir, "install.log");
            File.AppendAllText(filePath, $"{DateTime.Now:u} - Installed with greeting: {greeting}{Environment.NewLine}");

            Console.WriteLine("Post-install actions completed successfully.");
            return 0;
        }

        private static int RunUninstallActions(string[] args)
        {
            Console.WriteLine("Running uninstall actions...");

            const string registryPath = @"SOFTWARE\AdvancedInstallerLab";

            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true);
            if (key != null)
            {
                key.DeleteValue("GreetingMessage", throwOnMissingValue: false);
            }

            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string dir = Path.Combine(programData, "AdvancedInstallerLab");
            string filePath = Path.Combine(dir, "install.log");

            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, $"{DateTime.Now:u} - Uninstalled.{Environment.NewLine}");
            }

            Console.WriteLine("Uninstall actions completed successfully.");
            return 0;
        }
    }
}
