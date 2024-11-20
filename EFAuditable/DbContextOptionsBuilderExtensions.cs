using Microsoft.EntityFrameworkCore;

namespace EFAuditable
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddAudit(this DbContextOptionsBuilder optionsBuilder, IIdentityProvider identityProvider, TimeProvider? timeProvider = null)
        {
            return optionsBuilder.AddInterceptors(new AuditableInterceptor(identityProvider, timeProvider ?? TimeProvider.System));
        }
    }
}
