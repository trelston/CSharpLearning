namespace EntityFrameworksDocs;

public class DbContextOptions
{
    /*
        https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#dbcontextoptions
        DbContextOptions
    */

    /*
    The starting point for all DbContext configuration is DbContextOptionsBuilder. 
    There are three ways to get this builder:

        - In AddDbContext and related methods
        - In OnConfiguring
        - Constructed explicitly with new

    Examples of each of these are shown in the preceding sections. 

    The same configuration can be applied regardless of where the builder comes from. 

    In addition, OnConfiguring is always called regardless of how the context is constructed. 

    This means OnConfiguring can be used to perform additional configuration even when AddDbContext is being used.
    */
}