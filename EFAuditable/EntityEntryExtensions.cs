using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFAuditable
{
    public static class EntityEntryExtensions
    {
        public static EntityTypeBuilder Audit(this EntityTypeBuilder builder, bool audit = true, bool history = false)
        {
            builder.Metadata.SetAnnotation(AuditableAnnotations.Audit, audit);
            builder.Metadata.SetAnnotation(AuditableAnnotations.History, history);
            return builder;
        }

        public static bool IsAudit(this EntityEntry entry)
        {
            var annotation = entry.Metadata.FindAnnotation(AuditableAnnotations.Audit);
            var audit = annotation?.Value;

            if (audit is bool)
            {
                return audit.Equals(true);
            }

            return typeof(IAuditable).IsAssignableFrom(entry.Metadata.ClrType);
        }

        public static bool IsAudit(this IReadOnlyTypeBase entry)
        {
            var annotation = entry.FindAnnotation(AuditableAnnotations.Audit);
            var audit = annotation?.Value;

            if (audit is bool)
            {
                return audit.Equals(true);
            }

            return typeof(IAuditable).IsAssignableFrom(entry.ClrType);
        }

        public static bool IsHistory(this IReadOnlyTypeBase entry)
        {
            var annotation = entry.FindAnnotation(AuditableAnnotations.History);
            var history = annotation?.Value;

            if (history is bool)
            {
                return history.Equals(true);
            }

            return false;
        }
    }
}
