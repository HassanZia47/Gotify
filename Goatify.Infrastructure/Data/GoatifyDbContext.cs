using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Goatify.Core.Models;

namespace Goatify.Infrastructure.Data
{
    public class GoatifyDbContext : IdentityDbContext<AppUser>
    {
        public GoatifyDbContext(
        DbContextOptions<GoatifyDbContext> options)
        : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Goat> Goats { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<GoatMedication> GoatMedications { get; set; }
        public DbSet<Breeding> Breedings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fix decimal precision warnings
            modelBuilder.Entity<Expense>()
                .Property(e => e.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Unit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .Property(e => e.PricePerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Expense>()
                .HasQueryFilter(x => !x.DeletedFlag);


            modelBuilder.Entity<Goat>()
                .HasQueryFilter(x => !x.DeletedFlag);

            modelBuilder.Entity<Goat>()
                .Property(g => g.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Goat>()
                .Property(g => g.SalePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Goat>()
                .Property(g => g.PurchasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Bill>()
                .Property(g => g.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Bill>()
                .HasQueryFilter(x => !x.DeletedFlag);

            modelBuilder.Entity<Medication>()
                .HasQueryFilter(x => !x.DeletedFlag);

            modelBuilder.Entity<Breeding>()
                .HasQueryFilter(x => !x.DeletedFlag);

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseAuditEntity>();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDateTime = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDateTime = now;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedFlag = true;
                        entry.Entity.DeletedDateTime = now;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }

}