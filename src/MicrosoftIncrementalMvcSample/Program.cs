using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MicrosoftIncrementalMvcSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TelemetryDebugWriter.IsTracingDisabled = true;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
