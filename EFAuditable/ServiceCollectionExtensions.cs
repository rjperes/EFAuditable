using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EFAuditable
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuditableSerializer(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddSingleton<IAuditableSerializer, JsonAuditableSerializer>();
        }

        public static IServiceCollection AddAuditableSerializer<TService>(this IServiceCollection services) where TService : class, IAuditableSerializer
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddSingleton<IAuditableSerializer, TService>();
        }

        public static IServiceCollection AddAuditableSerializer<TService>(this IServiceCollection services, Func<IServiceProvider, IAuditableSerializer> factory) where TService : class, IAuditableSerializer
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddSingleton<IAuditableSerializer>(factory);
        }


        public static IServiceCollection AddAuditable(this IServiceCollection services, bool history, string historyTableName = null)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            services.AddOptions<AuditableOptions>().Configure(options =>
            {
                options.History = history;
                options.HistoryTableName = historyTableName ?? nameof(AuditableHistory);
            });
            return services;
        }

        public static IServiceCollection AddAuditable(this IServiceCollection services, Action<AuditableOptions> options)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            services.AddOptions<AuditableOptions>().Configure(options);
            return services;
        }

        public static IServiceCollection AddAuditableIdentityProvider(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            var asm = Assembly.GetEntryAssembly();
            var isWeb = (asm == null)
                || (asm.EntryPoint == null) && (Path.GetExtension(asm.Location).Equals(".dll", StringComparison.OrdinalIgnoreCase));

            if (isWeb)
            {
                services.AddSingleton<IIdentityProvider, WebIdentityProvider>();
            }
            else
            {
                services.AddSingleton<IIdentityProvider, ConsoleIdentityProvider>();
            }

            return services;
        }

        public static IServiceCollection AddAuditableIdentityProvider(this IServiceCollection services, Func<IServiceProvider, IIdentityProvider> factory)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            return services.AddSingleton<IIdentityProvider>(factory);
        }

        public static IServiceCollection AddAuditableIdentityProvider<TService>(this IServiceCollection services) where TService : class, IIdentityProvider
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            return services.AddSingleton<IIdentityProvider, TService>();
        }

        public static IServiceCollection AddAuditableTimeProvider(this IServiceCollection services)
        {
            return services.AddSingleton<TimeProvider>(TimeProvider.System);
        }

        public static IServiceCollection AddAuditableTimeProvider<TService>(this IServiceCollection services) where TService : TimeProvider
        {
            return services.AddSingleton<TimeProvider, TService>();
        }

        public static IServiceCollection AddAuditableTimeProvider(this IServiceCollection services, Func<IServiceProvider, TimeProvider> factory)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            return services.AddSingleton<TimeProvider>(factory);
        }
    }
}
