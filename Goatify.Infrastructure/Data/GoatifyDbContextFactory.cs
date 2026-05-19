using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Goatify.Infrastructure.Data;

public class GoatifyDbContextFactory
    : IDesignTimeDbContextFactory<GoatifyDbContext>
{
    public GoatifyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<GoatifyDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("GOATIFY_MIGRATION_CONNECTION_STRING")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=Goatify;Trusted_Connection=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new GoatifyDbContext(optionsBuilder.Options);
    }
}
