using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Ididit.WebView.Electron;

public class Program
{
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            try
            {
                string? message = error.ExceptionObject.ToString();

                System.Diagnostics.Debug.WriteLine(message);

                System.IO.File.WriteAllText("Error.log", message);
            }
            catch
            {
            }
        };

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseElectron(args);
                webBuilder.UseStartup<Startup>();
            });
}
