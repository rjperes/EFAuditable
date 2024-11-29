using Microsoft.EntityFrameworkCore;

namespace EFAuditable.Model
{
    public class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddAudit(static options =>
            {
                options.History = true;
            });
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Test>().Audit().IgnoreAuditFor(x => x.Name);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Test> Tests { get; private set; } = default!;
    }
}