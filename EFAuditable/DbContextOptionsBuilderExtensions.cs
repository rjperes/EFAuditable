using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFAuditable
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddAudit(this DbContextOptionsBuilder optionsBuilder, IIdentityProvider? identityProvider = null, TimeProvider? timeProvider = null)
        {
            return AddAudit(optionsBuilder, (options) => { }, identityProvider, timeProvider);
        }

        public static DbContextOptionsBuilder AddAudit(this DbContextOptionsBuilder optionsBuilder, Action<AuditableOptions> options, IIdentityProvider? identityProvider = null, TimeProvider? timeProvider = null, IAuditableSerializer? serializer = null)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var opt = new AuditableOptions();
            options.Invoke(opt);

            identityProvider = GetService(optionsBuilder, identityProvider);
            timeProvider = GetService(optionsBuilder, timeProvider, TimeProvider.System);
            serializer = GetService(optionsBuilder, serializer, new JsonAuditableSerializer());

            var logger = GetService<ILogger<AuditableInterceptor>>(optionsBuilder, null);

            return optionsBuilder.AddInterceptors(new AuditableInterceptor(opt, identityProvider, timeProvider, serializer, logger));
        }

        private static T GetService<T>(DbContextOptionsBuilder optionsBuilder, T? service, T? defaultValue = default) where T : class
        {
            var ext = optionsBuilder.Options.FindExtension<CoreOptionsExtension>();
            service ??= ext!.ApplicationServiceProvider!.GetService<T>() ?? defaultValue;
            return service!;
        }
    }
}
