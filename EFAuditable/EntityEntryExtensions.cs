using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace EFAuditable
{
    public static class EntityEntryExtensions
    {
        public class AuditEntityTypeBuilder<T>(EntityTypeBuilder<T> builder) where T : class
        {
            public AuditEntityTypeBuilder<T> IgnoreProperty<TMember>(Expression<Func<T, TMember>> property)
            {
                ArgumentNullException.ThrowIfNull(property, nameof(property));

                if (property.Body is MemberExpression member)
                {
                    builder.Property(member.Member.Name).Metadata.AddAnnotation(AuditableAnnotations.IgnoreProperty, true);
                }

                return this;
            }
        }

        public static AuditEntityTypeBuilder<T> Audit<T>(this EntityTypeBuilder<T> builder, Action<AuditableOptions> options) where T : class
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var opt = new AuditableOptions();
            options(opt);

            return Audit(builder, true, opt.History);
        }

        public static AuditEntityTypeBuilder<T> Audit<T>(this EntityTypeBuilder<T> builder, bool audit = true, bool history = false) where T : class
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata.SetAnnotation(AuditableAnnotations.Audit, audit);
            builder.Metadata.SetAnnotation(AuditableAnnotations.History, history);
            return new(builder);
        }

        public static bool IsAudit(this EntityEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
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
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
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
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
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