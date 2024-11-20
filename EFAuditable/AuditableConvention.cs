using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFAuditable
{
    internal class AuditableConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().Where(entityType => typeof(IAuditable).IsAssignableFrom(entityType.ClrType)))
            {
                entityType.AddProperty(nameof(IAuditable.CreatedBy), typeof(string));
                entityType.AddProperty(nameof(IAuditable.CreatedAt), typeof(DateTimeOffset?));
                entityType.AddProperty(nameof(IAuditable.UpdatedBy), typeof(string));
                entityType.AddProperty(nameof(IAuditable.UpdatedAt), typeof(DateTimeOffset?));
            }
        }
    }
}