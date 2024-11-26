using EFAuditable.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            services.AddLogging(options =>
            {
                options.AddConfiguration(configuration.GetSection("Logging"));
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

            var cs = ctx.GetInfrastructure().GetService<IDbContextServices>();
            //var cd = ctx.GetDependencies();
            //var co = ctx.GetInfrastructure().GetService<IDbContextOptions>();
            //var oe = co.FindExtension<CoreOptionsExtension>();
            //var soe = co.FindExtension<SqlServerOptionsExtension>();
           
            //var m = (cs.Model as RuntimeModel);
            //var rm = m.GetRelationalModel();
            //var dtm = cs.DesignTimeModel;
            //var mm = (dtm as Microsoft.EntityFrameworkCore.Metadata.Internal.Model);
            //var cds = mm.Builder;

            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("rjperes"), []);

            var tests = ctx.Tests.ToList();

            var test = tests.First();

            test.Name = $"Test Updated at {DateTime.UtcNow}";

            ctx.Add(new Test { Name = "ABC" });

            //ctx.Tests.Remove(ctx.Tests.First(x => x.Name == "ABC"));

            ctx.SaveChanges();

            var audits = ctx.Audits().ToList();
        }
    }
}
