using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Principal;

namespace EFAuditable
{
    internal class AuditableInterceptor(AuditableOptions Options, IIdentityProvider IdentityProvider, TimeProvider TimeProvider, IAuditableSerializer Serializer) : SaveChangesInterceptor
    {
        private void SetModifiedProperties(EntityEntry entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(IAuditable.CreatedBy)] = identity;
                entry.CurrentValues[nameof(IAuditable.CreatedAt)] = time;
            }
        }

        private void SetAddedProperties(EntityEntry entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Modified)
            {
                entry.CurrentValues[nameof(IAuditable.UpdatedBy)] = identity;
                entry.CurrentValues[nameof(IAuditable.UpdatedAt)] = time;
            }
        }

        protected IEnumerable<EntityEntry> DoSaveChanges(DbContext context)
        {
            var time = TimeProvider.GetUtcNow();
            var identity = IdentityProvider.GetCurrentUser();

            var entries = context.ChangeTracker.Entries().Where(x => x.IsAudit()).Where(x => x.State == EntityState.Added || x.State == EntityState.Modified).ToList();
            var newEntries = entries.Where(x => x.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (Options.History)
                {
                    var key = entry.Metadata.FindPrimaryKey();
                    var props = key?.Properties?.ToDictionary(x => x.Name, x => entry.Property(x.Name).OriginalValue);
                    CopyOldProperties(context, entry, time, identity, props ?? new());
                }

                SetModifiedProperties(entry, time, identity);
                SetAddedProperties(entry, time, identity);
            }

            return newEntries;
        }

        private void DoSaveHistory(DbContext context, IEnumerable<EntityEntry> newEntries)
        {
            var time = TimeProvider.GetUtcNow();
            var identity = IdentityProvider.GetCurrentUser();

            foreach (var entry in newEntries)
            {
                if (Options.History)
                {
                    var key = entry.Metadata.FindPrimaryKey();
                    var props = key?.Properties?.ToDictionary(x => x.Name, x => entry.Property(x.Name).OriginalValue);
                    CopyOldProperties(context, entry, time, identity, props ?? new());
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var newEntries = DoSaveChanges(eventData.Context!);
            var res = base.SavingChanges(eventData, result);
            DoSaveHistory(eventData.Context!, newEntries);
            base.SavingChanges(eventData, result);
            return res;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var newEntries = DoSaveChanges(eventData.Context!);
            var res = await base.SavingChangesAsync(eventData, result, cancellationToken);
            DoSaveHistory(eventData.Context!, newEntries);
            await base.SavingChangesAsync(eventData, result, cancellationToken);
            return res;
        }

        private void CopyOldProperties(DbContext ctx, EntityEntry entry, DateTimeOffset time, string identity, Dictionary<string, object?> key)
        {
            if (Options.History)
            {
                var keys = Serializer.Serialize(key);
                var values = Serializer.Serialize(entry.Entity);

                var history = new AuditableHistory
                {
                    Key = keys,
                    Entity = entry.Metadata.Name,
                    Values = values,
                    Timestamp = time,
                    User = identity
                };

                ctx.Add(history);
            }
        }
    }
}