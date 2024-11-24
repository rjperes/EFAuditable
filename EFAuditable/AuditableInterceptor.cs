using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace EFAuditable
{
    internal class AuditableInterceptor(AuditableOptions Options, IIdentityProvider IdentityProvider, TimeProvider TimeProvider, IAuditableSerializer Serializer, ILogger<AuditableInterceptor> Logger) : SaveChangesInterceptor
    {
        private void SetModifiedProperties(EntityEntry entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(IAuditable.CreatedBy)] = identity;
                entry.CurrentValues[nameof(IAuditable.CreatedAt)] = time;

                Logger.LogDebug($"Setting audit properties for new {entry.Metadata.Name}");
            }
        }

        private void SetAddedProperties(EntityEntry entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Modified)
            {
                entry.CurrentValues[nameof(IAuditable.UpdatedBy)] = identity;
                entry.CurrentValues[nameof(IAuditable.UpdatedAt)] = time;

                Logger.LogDebug($"Setting audit properties for existing {entry.Metadata.Name}");
            }
        }

        protected void DoSaveChanges(DbContext context)
        {
            var time = TimeProvider.GetUtcNow();
            var identity = IdentityProvider.GetCurrentUser();

            var entries = context.ChangeTracker.Entries().Where(x => x.IsAudit()).Where(x => x.State == EntityState.Added || x.State == EntityState.Modified).ToList();

            foreach (var entry in entries)
            {
                SetModifiedProperties(entry, time, identity);
                SetAddedProperties(entry, time, identity);
            }
        }

        private void DoSaveHistory(DbContext context)
        {
            if (Options.History)
            {
                var time = TimeProvider.GetUtcNow();
                var identity = IdentityProvider.GetCurrentUser();

                var entries = context.ChangeTracker.Entries().Where(x => x.IsAudit()).Where(x => x.State == EntityState.Modified).ToList();

                foreach (var entry in entries)
                {
                    var key = entry.Metadata.FindPrimaryKey();
                    var props = key?.Properties?.ToDictionary(x => x.Name, x => entry.Property(x.Name).OriginalValue);
                    CopyOldProperties(context, entry, time, identity, props ?? new());
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            DoSaveHistory(eventData.Context!);
            base.SavingChanges(eventData, result);
            DoSaveChanges(eventData.Context!);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            DoSaveHistory(eventData.Context!);
            await base.SavingChangesAsync(eventData, result, cancellationToken);
            DoSaveChanges(eventData.Context!);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CopyOldProperties(DbContext ctx, EntityEntry entry, DateTimeOffset time, string identity, Dictionary<string, object?> key)
        {
            if (Options.History)
            {
                var ignoreProperties = entry.Properties
                    .Where(x => object.Equals(x.Metadata.FindAnnotation(AuditableAnnotations.IgnoreProperty)?.Value, true))
                    .Select(x => x.Metadata.Name)
                    .Concat([nameof(IAuditable.UpdatedAt), nameof(IAuditable.UpdatedBy), nameof(IAuditable.CreatedAt), nameof(IAuditable.CreatedBy)])
                    .ToArray();
                var keys = Serializer.Serialize(key);
                var values = Serializer.Serialize(entry.Entity, ignoreProperties);

                var history = new AuditableHistory
                {
                    Key = keys,
                    Entity = entry.Metadata.Name,
                    Values = values,
                    Timestamp = time,
                    User = identity,
                    State = entry.State.ToString()
                };

                ctx.Add(history);

                Logger.LogDebug($"Adding audit history for existing {entry.Metadata.Name}");
            }
        }
    }
}