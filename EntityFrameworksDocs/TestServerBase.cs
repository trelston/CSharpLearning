using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EntityFrameworksDocs;

public class TestServerBase
{
    //public TestServer CreateServer()
    //{
    //    var path = Assembly.GetAssembly(typeof(TestServerBase))
    //        .Location;

    //    var hostBuilder = new WebHostBuilder()
    //        .UseContentRoot(Path.GetDirectoryName(path))
    //        .ConfigureAppConfiguration(cb =>
    //        {
    //            cb.AddJsonFile("appsettings.json", optional: false)
    //                .AddEnvironmentVariables();
    //        })
    //        .UseStartup<Startup>();


    //    var testServer = new TestServer(hostBuilder);

    //   return testServer;
    //}
}