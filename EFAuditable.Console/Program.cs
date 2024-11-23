using EFAuditable.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Principal;

namespace EFAuditable.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            var services = new ServiceCollection();
            services.AddAuditableTimeProvider();
            services.AddAuditableIdentityProvider();
            services.AddAuditable(history: true);
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
                options.AddAudit();
            });

            var serviceProvider = services.BuildServiceProvider(true);

            using var scope = serviceProvider.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("rjperes"), Array.Empty<string>());

            var cs = ctx.GetInfrastructure().GetService<IDbContextServices>();
            var cd = ctx.GetInfrastructure().GetService<IDbContextDependencies>();

            var tests = ctx.Tests.ToList();

            var test = tests.First();

            test.Name = $"Test Updated at {DateTime.UtcNow}";

            ctx.Add(new Test { Name = "ABC" });

            ctx.SaveChanges();
        }
    }
}
