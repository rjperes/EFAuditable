using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFAuditable
{
    internal class AuditableConvention(AuditableOptions options) : IModelFinalizingConvention, IModelInitializedConvention
    {
        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            AddToModel(modelBuilder);
        }

        public void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            AddToModel(modelBuilder);
        }

        private void AddToModel(IConventionModelBuilder modelBuilder)
        {
            if (options.History)
            {
                if (!modelBuilder.Metadata.GetEntityTypes().Any(x => x.ClrType == typeof(AuditableHistory)))
                {
                    var entityBuilder = modelBuilder.Entity(typeof(AuditableHistory));
                    entityBuilder!.ToTable(string.IsNullOrWhiteSpace(options.HistoryTableName) ? nameof(AuditableHistory) : options.HistoryTableName);
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.Id), typeof(int), true);
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.Key), typeof(string));
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.Entity), typeof(string));
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.Values), typeof(string));
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.Timestamp), typeof(DateTimeOffset));
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.User), typeof(string));
                    EnsureProperty(entityBuilder!, nameof(AuditableHistory.State), typeof(string));
                }
            }

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().Where(x => x.IsAudit()))
            {
                EnsureProperty(entityType, nameof(IAuditable.CreatedBy), typeof(string));
                EnsureProperty(entityType, nameof(IAuditable.CreatedAt), typeof(DateTimeOffset?));
                EnsureProperty(entityType, nameof(IAuditable.UpdatedBy), typeof(string));
                EnsureProperty(entityType, nameof(IAuditable.UpdatedAt), typeof(DateTimeOffset?));
            }
        }

        private static void EnsureProperty(IConventionEntityTypeBuilder entityTypeBuilder, string name, Type type, bool primaryKey = false)
        {
            var prop = entityTypeBuilder.Property(type, name);

            if (primaryKey)
            {
                prop!.Metadata.IsPrimaryKey();
            }
        }

        private static void EnsureProperty(IConventionEntityType entityType, string name, Type type)
        {
            if (entityType.FindProperty(name) == null)
            {
                entityType.AddProperty(name, type);
            }
        }
    }
}