using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFAuditable
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddAudit(this DbContextOptionsBuilder optionsBuilder, IIdentityProvider? identityProvider = null, TimeProvider? timeProvider = null)
        {
            return AddAudit(optionsBuilder, null, identityProvider, timeProvider);
        }

        public static DbContextOptionsBuilder AddAudit(this DbContextOptionsBuilder optionsBuilder, Action<AuditableOptions>? options, IIdentityProvider? identityProvider = null, TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(identityProvider, nameof(identityProvider));

            var opt = new AuditableOptions();
            options?.Invoke(opt);

            optionsBuilder.Options.WithExtension(new AuditableExtension());

            var ext = optionsBuilder.Options.FindExtension<CoreOptionsExtension>();
            identityProvider = identityProvider ?? ext!.ApplicationServiceProvider!.GetRequiredService<IIdentityProvider>();
            timeProvider = timeProvider ?? ext!.ApplicationServiceProvider!.GetRequiredService<TimeProvider>();

            return optionsBuilder.AddInterceptors(new AuditableInterceptor(opt, identityProvider, timeProvider ?? TimeProvider.System));
        }        
    }
}
