using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFAuditable
{
    public static class ModelConfigurationBuilderExtensions
    {
        public static ModelConfigurationBuilder AddAudit(this ModelConfigurationBuilder configurationBuilder, Action<AuditableOptions>? options = null)
        {
            configurationBuilder.Conventions.Add(sp => new AuditableConvention(GetOptions(sp, options)));
            return configurationBuilder;
        }

        private static AuditableOptions GetOptions(IServiceProvider serviceProvider, Action<AuditableOptions>? options)
        {
            AuditableOptions opt;

            if (options != null)
            {
                opt = new AuditableOptions();
                options(opt);
            }
            else
            {
                opt = serviceProvider.GetService<IOptions<AuditableOptions>>()?.Value ?? new();
            }

            return opt;
        }
    }
}
