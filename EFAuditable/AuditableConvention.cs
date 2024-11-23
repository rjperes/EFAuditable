using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFAuditable
{
    internal class AuditableConvention(AuditableOptions Options) : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            if (Options.StoreOldValues)
            {
                if (!modelBuilder.Metadata.GetEntityTypes().Any(x => x.ClrType == typeof(AuditableHistory)))
                {
                    var entityType = modelBuilder.Metadata.AddEntityType(string.IsNullOrWhiteSpace(Options.OldValuesTableName) ? nameof(AuditableHistory) : Options.OldValuesTableName, typeof(AuditableHistory));
                    CheckProperty(entityType, nameof(AuditableHistory.Id), typeof(Guid), true);
                    CheckProperty(entityType, nameof(AuditableHistory.Key), typeof(string));
                    CheckProperty(entityType, nameof(AuditableHistory.Entity), typeof(string));
                    CheckProperty(entityType, nameof(AuditableHistory.Values), typeof(string));
                    CheckProperty(entityType, nameof(AuditableHistory.Timestamp), typeof(DateTimeOffset));
                    CheckProperty(entityType, nameof(AuditableHistory.User), typeof(string));
                }
            }
 
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().Where(x => x.IsAudit()))
            {
                CheckProperty(entityType, nameof(IAuditable.CreatedBy), typeof(string));
                CheckProperty(entityType, nameof(IAuditable.CreatedAt), typeof(DateTimeOffset?));
                CheckProperty(entityType, nameof(IAuditable.UpdatedBy), typeof(string));
                CheckProperty(entityType, nameof(IAuditable.UpdatedAt), typeof(DateTimeOffset?));
            }
        }

        private void CheckProperty(IConventionEntityType entityType, string name, Type type, bool primaryKey = false)
        {
            if (entityType.GetProperty(name) == null)
            {
                entityType.AddProperty(name, type);
            }

            if (primaryKey)
            {
                entityType.SetPrimaryKey(entityType.GetProperty(name));
            }
        }
    }
}