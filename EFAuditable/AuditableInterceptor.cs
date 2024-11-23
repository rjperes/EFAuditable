using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EFAuditable
{
    internal class AuditableInterceptor(AuditableOptions Options, IIdentityProvider IdentityProvider, TimeProvider TimeProvider) : SaveChangesInterceptor
    {
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            MaxDepth = 1,
            IncludeFields = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private void SetModifiedProperties(EntityEntry<IAuditable> entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entry.Property<string?>(nameof(IAuditable.CreatedBy)).CurrentValue = identity;
                entry.Property<DateTimeOffset?>(nameof(IAuditable.CreatedAt)).CurrentValue = time;
            }
        }

        private void SetAddedProperties(EntityEntry<IAuditable> entry, DateTimeOffset time, string identity)
        {
            var entity = entry.Entity;

            if (entry.State == EntityState.Modified)
            {
                entry.Property<string?>(nameof(IAuditable.UpdatedBy)).CurrentValue = identity;
                entry.Property<DateTimeOffset?>(nameof(IAuditable.UpdatedAt)).CurrentValue = time;
            }
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var time = TimeProvider.GetUtcNow();
            var identity = IdentityProvider.GetCurrentUser();

            foreach (var entry in eventData.Context!.ChangeTracker.Entries<IAuditable>().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
            {
                if (Options.StoreOldValues)
                {
                    var key = entry.Metadata.FindPrimaryKey();
                    var props = key?.Properties?.ToDictionary(x => x.Name, x => entry.Property(x.Name).CurrentValue);
                    CopyOldProperties(eventData.Context, entry, time, identity, props ?? new());
                }

                SetModifiedProperties(entry, time, identity);
                SetAddedProperties(entry, time, identity);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void CopyOldProperties(DbContext ctx, EntityEntry<IAuditable> entry, DateTimeOffset time, string identity, Dictionary<string, object?> key)
        {
            if (Options.StoreOldValues)
            {
                var history = new AuditableHistory
                {
                    Id = Guid.NewGuid(),
                    Key = (key.Count == 1) ? key.First().Value!.ToString()! : JsonSerializer.Serialize(key, _serializerOptions),
                    Entity = entry.Metadata.Name,
                    Values = JsonSerializer.Serialize(entry.Entity, _serializerOptions),
                    Timestamp = time,
                    User = identity
                };

                ctx.Add(history);
            }
        }
    }
}