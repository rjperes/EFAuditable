using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EFAuditable
{
    internal class AuditableConvention(AuditableOptions Options) : IModelFinalizingConvention, IModelInitializedConvention
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
            if (Options.History)
            {
                if (!modelBuilder.Metadata.GetEntityTypes().Any(x => x.ClrType == typeof(AuditableHistory)))
                {
                    var entityBuilder = modelBuilder.Entity(typeof(AuditableHistory));
                    entityBuilder.ToTable(string.IsNullOrWhiteSpace(Options.HistoryTableName) ? nameof(AuditableHistory) : Options.HistoryTableName);
                    CheckProperty(entityBuilder, nameof(AuditableHistory.Id), typeof(int), true);
                    CheckProperty(entityBuilder, nameof(AuditableHistory.Key), typeof(string));
                    CheckProperty(entityBuilder, nameof(AuditableHistory.Entity), typeof(string));
                    CheckProperty(entityBuilder, nameof(AuditableHistory.Values), typeof(string));
                    CheckProperty(entityBuilder, nameof(AuditableHistory.Timestamp), typeof(DateTimeOffset));
                    CheckProperty(entityBuilder, nameof(AuditableHistory.User), typeof(string));
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

        private void CheckProperty(IConventionEntityTypeBuilder entityTypeBuilder, string name, Type type, bool primaryKey = false)
        {
            var prop = entityTypeBuilder.Property(type, name);

            if (primaryKey)
            {
                prop!.Metadata.IsPrimaryKey();
            }
        }

        private void CheckProperty(IConventionEntityType entityType, string name, Type type, bool primaryKey = false)
        {
            if (entityType.FindProperty(name) == null)
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