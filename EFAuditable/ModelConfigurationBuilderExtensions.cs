using Microsoft.EntityFrameworkCore;

namespace EFAuditable
{
    public static class ModelConfigurationBuilderExtensions
    {
        public static ModelConfigurationBuilder AddAudit(this ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new AuditableConvention());
            return configurationBuilder;
        }
    }
}
