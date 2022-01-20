using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworksDocs;

public class SimpleDbContextInitializationWithNew
{
	/*
        https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#simple-dbcontext-initialization-with-new
        Simple DbContext initialization with 'new'
    */

    /*
        DbContext instances can be constructed in the normal .NET way, for example with new in C#.

        Configuration can be performed by overriding the OnConfiguring method, or by passing options to the constructor.
        For example:
    */
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test");
        }
    }

    /*
        This pattern also makes it easy to pass configuration like the connection string via the DbContext constructor. 
        For example:
    */
    public class ApplicationDbContext1 : DbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext1(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }


    /*
        Alternately, DbContextOptionsBuilder can be used to create a DbContextOptions object that is then passed to the 
        DbContext constructor. 

        This allows a DbContext configured for dependency injection to also be constructed explicitly.

        For example, when using ApplicationDbContext defined for ASP.NET Core web apps above:
    */

    public class ApplicationDbContext2 : DbContext
    {
        public ApplicationDbContext2(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    [Fact]
    public void TestMain()
    {
        /*
	        The DbContextOptions can be created and the constructor can be called explicitly:
	    */
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test")
            .Options;

        using var context = new ApplicationDbContext2(contextOptions);
    }
}