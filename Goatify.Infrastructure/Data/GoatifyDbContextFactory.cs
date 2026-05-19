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

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=Goatify;Trusted_Connection=True;");

        return new GoatifyDbContext(optionsBuilder.Options);
    }
}