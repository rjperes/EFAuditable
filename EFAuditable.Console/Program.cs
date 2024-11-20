using EFAuditable.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddSingleton<IIdentityProvider, ConsoleIdentityProvider>();
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
            });

            var serviceProvider = services.BuildServiceProvider(true);

            using var scope = serviceProvider.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();

            ctx.Tests.ToList();
        }
    }
}
