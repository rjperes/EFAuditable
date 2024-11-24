using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFAuditable
{
    public class AuditableExtension(AuditableOptions Options) : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info => null;

        public void ApplyServices(IServiceCollection services)
        {

        }

        public void Validate(IDbContextOptions options)
        {

        }
    }
}