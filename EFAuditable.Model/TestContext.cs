using Microsoft.EntityFrameworkCore;

namespace EFAuditable.Web
{
    public class TestDbContext : DbContext
    {
        private readonly TimeProvider _timeProvider;
        private readonly IIdentityProvider _identityProvider;

        public TestDbContext(DbContextOptions options, TimeProvider timeProvider, IIdentityProvider identityProvider) : base(options)
        {
            _timeProvider = timeProvider;
            _identityProvider = identityProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddAudit(_identityProvider, _timeProvider);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddAudit();
            base.ConfigureConventions(configurationBuilder);
        }
    }
}