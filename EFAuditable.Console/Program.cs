using EFAuditable.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Principal;

namespace EFAuditable.Console
{
    internal class Program
    {
        static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(static options =>
            {
                options.AddConsole();
            });
            services.AddAuditableTimeProvider();
            services.AddAuditableIdentityProvider();
            services.AddAuditable(history: true);
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
                options.AddAudit(static options =>
                {
                    options.History = true;
                });
            });

            var serviceProvider = services.BuildServiceProvider(true);

            using var scope = serviceProvider.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("rjperes"), []);

            var tests = ctx.Tests.ToList();

            var test = tests.First();

            test.Name = $"Test Updated at {DateTime.UtcNow}";

            ctx.Add(new Test { Name = "ABC" });

            ctx.SaveChanges();
        }
    }
}
