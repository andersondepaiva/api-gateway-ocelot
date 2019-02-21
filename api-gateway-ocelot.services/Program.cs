namespace api_gateway_ocelot.services
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Ocelot.DependencyInjection;
    using Ocelot.Middleware;
    using api_gateway_ocelot.services.tracing;

    public class Program
    {
        private const string basePathOcelotConfig = "./ocelot-config/";
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return new WebHostBuilder()
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"{basePathOcelotConfig}ocelot.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json", false, true)
                .AddEnvironmentVariables();

                configuration = config.Build();
            })
            .ConfigureServices(s =>
            {
                s.AddOcelot()
                .AddButterfly(option =>
                {
                    option.CollectorUrl = configuration.GetSection("ButterflyTrace").GetSection("Server").Value;
                    option.Service = configuration.GetSection("ButterflyTrace").GetSection("Application").Value;

                });
            })
            .UseIISIntegration()
            .Configure(app =>
            {
                app.UseOcelot().Wait();
            })
            .Build();
        }
    }
}
