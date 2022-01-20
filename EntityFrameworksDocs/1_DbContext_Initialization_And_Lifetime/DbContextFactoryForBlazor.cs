using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworksDocs;

public class DbContextFactoryForBlazor
{
    /*
        https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor
        Using a DbContext factory (e.g. for Blazor)
    */

    /*
        Some application types (e.g. ASP.NET Core Blazor) use dependency injection but do not create a service scope 
        that aligns with the desired DbContext lifetime. 

        Even where such an alignment does exist, the application may need to perform multiple units-of-work within this scope.

        For example, multiple units-of-work within a single HTTP request.
    */

    /*
        In these cases, AddDbContextFactory can be used to register a factory for creation of DbContext instances.
        For example:
    */
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        /*
            ASP.NET Core applications are configured using dependency injection. 
            EF Core can be added to this configuration using AddDbContext in the ConfigureServices method 
            of Startup.cs. For example:
        */
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<ApplicationDbContext>(
                options =>
                    options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test"));
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
}

/*
    The ApplicationDbContext class must expose a public constructor with a DbContextOptions<ApplicationDbContext> parameter. 
    This is the same pattern as used in the traditional ASP.NET Core section above.
*/
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

/*
    The DbContextFactory factory can then be used in other services through constructor injection. For example:
*/
class MyController : ControllerBase
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public MyController(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /*
    The injected factory can then be used to construct DbContext instances in the service code. For example:
    */
    public void DoSomething()
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            // ...
        }
        /*
        Notice that the DbContext instances created in this way are not managed by the application's service provider 
        and therefore must be disposed by the application.
        */
    }
}