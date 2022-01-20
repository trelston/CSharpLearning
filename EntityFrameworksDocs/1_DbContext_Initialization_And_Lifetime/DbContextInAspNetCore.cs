using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityFrameworksDocs;

public class DbContextInAspNetCore
{
    public TestServer CreateServer()
    {
        var path = Assembly.GetAssembly(typeof(DbContextInAspNetCore))
            .Location;

        var hostBuilder = new WebHostBuilder()
            .UseContentRoot(Path.GetDirectoryName(path))
            .ConfigureAppConfiguration(cb =>
            {
                cb.AddJsonFile("appsettings.json", false)
                    .AddEnvironmentVariables();
            })
            .UseStartup<Startup>();

        var testServer = new TestServer(hostBuilder);

        return testServer;
    }

    [Fact]
    public async Task Get_get_all_catalogitems_and_response_ok_status_code()
    {
        using (var server = CreateServer())
        {
            var response = await server.CreateClient()
                .GetAsync("weatherforecast");

            response.EnsureSuccessStatusCode();
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        /*
        ASP.NET Core applications are configured using dependency injection. 
        EF Core can be added to this configuration using AddDbContext in the ConfigureServices method 
        of Startup.cs. For example:
        */
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));
            /*
            This example registers a DbContext subclass called ApplicationDbContext as a scoped service 
            in the ASP.NET Core application service provider (a.k.a. the dependency injection container). 

            The context is configured to use the SQL Server database provider and will read the connection 
            string from ASP.NET Core configuration.

            It typically does not matter where in ConfigureServices the call to AddDbContext is made.
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }

    /*
    The ApplicationDbContext class must expose a public constructor 
    with a DbContextOptions<ApplicationDbContext> parameter. 

    This is how context configuration from AddDbContext is passed to the DbContext. For example:
    */
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
        public string Summary { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ApplicationDbContext _context;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            /*
                ApplicationDbContext can then be used in ASP.NET Core controllers or other services through constructor injection. 
                For example:
            */
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        /*
		    The final result is an ApplicationDbContext instance created for each request and 
		    passed to the controller to perform a unit-of-work before being disposed when the request ends.
		*/

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}