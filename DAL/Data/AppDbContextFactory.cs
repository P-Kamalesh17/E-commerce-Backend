using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Data
{
    // Lets `dotnet ef` build an AppDbContext for design-time commands (migrations add/list/etc.)
    // without running the API's Program.cs top-level startup code, which eagerly calls
    // Database.Migrate() and throws PendingModelChangesWarning whenever a migration is missing —
    // exactly the situation this tool is used to fix.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(
                "Data Source=.\\sqlexpress;Initial Catalog=Ecommerce;Integrated Security=True;Trust Server Certificate=True");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
