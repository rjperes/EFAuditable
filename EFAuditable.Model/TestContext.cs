using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFAuditable.Model
{
    [DbContext(typeof(TestDbContext))]
    public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlServer("Server=.; Database=Test; Integrated Security=SSPI; TrustServerCertificate=true;")
                .Options;
            return new TestDbContext(options);
        }
    }

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

        public DbSet<Test> Tests { get; private set; }
    }
}