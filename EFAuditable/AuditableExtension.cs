using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFAuditable
{
    public class AuditableExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info
        {
            get
            {
                return null;
            }
        }

        public void ApplyServices(IServiceCollection services)
        {
            
        }

        public void Validate(IDbContextOptions options)
        {
            
        }
    }
}