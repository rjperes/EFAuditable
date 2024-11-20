using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFAuditable
{
    internal class AuditableInterceptor(IIdentityProvider IdentityProvider, TimeProvider TimeProvider) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var time = TimeProvider.GetUtcNow();
            var identity = IdentityProvider.GetCurrentUser();

            foreach (var entity in eventData.Context!.ChangeTracker.Entries<IAuditable>().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
            {
                if (entity.State == EntityState.Added)
                {
                    entity.Property<string>(nameof(IAuditable.CreatedBy)).CurrentValue = identity;
                    entity.Property<DateTimeOffset?>(nameof(IAuditable.CreatedAt)).CurrentValue = time;
                }

                if (entity.State == EntityState.Modified)
                {
                    entity.Property<string>(nameof(IAuditable.UpdatedBy)).CurrentValue = identity;
                    entity.Property<DateTimeOffset?>(nameof(IAuditable.UpdatedAt)).CurrentValue = time;
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}