using Goatify.Core.Models;
using Goatify.Core.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Goatify.Infrastructure.Data
{
    public class GoatifyDbContext : IdentityDbContext<AppUser>
    {
        private readonly ICurrentUserService? _currentUserService;

        public GoatifyDbContext(
            DbContextOptions<GoatifyDbContext> options)
            : this(options, null)
        {
        }

        public GoatifyDbContext(
            DbContextOptions<GoatifyDbContext> options,
            ICurrentUserService? currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
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
            var userName = _currentUserService?.UserName ?? "system";

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = userName;
                        entry.Entity.CreatedDateTime = now;
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.CreatedBy).IsModified = false;
                        entry.Property(x => x.CreatedDateTime).IsModified = false;

                        var deletedFlagChanged = entry.Property(x => x.DeletedFlag).IsModified;

                        if (deletedFlagChanged && entry.Entity.DeletedFlag)
                        {
                            entry.Entity.DeletedBy = userName;
                            entry.Entity.DeletedDateTime = now;
                        }
                        else if (deletedFlagChanged && !entry.Entity.DeletedFlag)
                        {
                            entry.Entity.DeletedBy = null;
                            entry.Entity.DeletedDateTime = null;
                        }

                        entry.Entity.ModifiedBy = userName;
                        entry.Entity.ModifiedDateTime = now;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Property(x => x.CreatedBy).IsModified = false;
                        entry.Property(x => x.CreatedDateTime).IsModified = false;
                        entry.Entity.DeletedFlag = true;
                        entry.Entity.DeletedBy = userName;
                        entry.Entity.DeletedDateTime = now;
                        entry.Entity.ModifiedBy = userName;
                        entry.Entity.ModifiedDateTime = now;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
