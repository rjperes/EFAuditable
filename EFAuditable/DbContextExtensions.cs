using Microsoft.EntityFrameworkCore;

namespace EFAuditable
{
    public static class DbContextExtensions
    {
        public static IQueryable<AuditableHistory> Audits(this DbContext context)
        {
            return context.Set<AuditableHistory>();
        }
    }
}
