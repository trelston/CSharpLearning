namespace EntityFrameworksDocs;

public class DbContextLifetime
{
    /*
        https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#the-dbcontext-lifetime
        The DbContext lifetime
    */

    /*
        The lifetime of a DbContext begins when the instance is created and ends when the instance is disposed. 
        A DbContext instance is designed to be used for a single unit-of-work. 
        This means that the lifetime of a DbContext instance is usually very short.
    */

    /*
        A typical unit-of-work when using Entity Framework Core (EF Core) involves:

            - Creation of a DbContext instance
            - Tracking of entity instances by the context. Entities become tracked by
                - Being returned from a query
                - Being added or attached to the context
            - Changes are made to the tracked entities as needed to implement the business rule
            - SaveChanges or SaveChangesAsync is called. EF Core detects the changes made and writes them 
                to the database.
            - The DbContext instance is disposed
    */

    /*
        Important

        - It is very important to dispose the DbContext after use. 
          This ensures both that any unmanaged resources are freed, and that any events or other hooks 
          are unregistered so as to prevent memory leaks in case the instance remains referenced.
        - DbContext is not thread-safe. Do not share contexts between threads. 
          Make sure to await all async calls before continuing to use the context instance.
        - An InvalidOperationException thrown by EF Core code can put the context into an unrecoverable state. 
          Such exceptions indicate a program error and are not designed to be recovered from.
    */

}