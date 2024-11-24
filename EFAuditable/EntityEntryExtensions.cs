using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace EFAuditable
{
    public static class EntityEntryExtensions
    {
        public static EntityTypeBuilder<T> IgnoreAuditFor<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, object>> property) where T : class
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            ArgumentNullException.ThrowIfNull(property, nameof(property));

            if (!(property.Body is MemberExpression member) || !member.Member.DeclaringType!.IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Invalid member expression.", nameof(property));
            }

            builder.Property(member.Member.Name).Metadata.AddAnnotation(AuditableAnnotations.IgnoreProperty, true);

            return builder;
        }

        public static EntityTypeBuilder<T> Audit<T>(this EntityTypeBuilder<T> builder, bool audit = true, bool history = false) where T : class
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata.SetAnnotation(AuditableAnnotations.Audit, audit);
            builder.Metadata.SetAnnotation(AuditableAnnotations.History, history);
            return builder;
        }

        internal static bool IsAudit(this EntityEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
            var audit = entry.Metadata.FindAnnotation(AuditableAnnotations.Audit)?.Value;

            if (audit is bool)
            {
                return audit.Equals(true);
            }

            return typeof(IAuditable).IsAssignableFrom(entry.Metadata.ClrType);
        }

        internal static bool IsAudit(this IReadOnlyTypeBase entry)
        {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));
            var audit = entry.FindAnnotation(AuditableAnnotations.Audit)?.Value;

            if (audit is bool)
            {
                return audit.Equals(true);
            }

            return typeof(IAuditable).IsAssignableFrom(entry.ClrType);
        }
    }
}