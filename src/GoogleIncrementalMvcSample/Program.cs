using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace GoogleIncrementalMvcSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TelemetryDebugWriter.IsTracingDisabled = true;
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
