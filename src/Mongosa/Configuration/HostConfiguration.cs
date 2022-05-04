using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mongosa.Interfaces;

namespace Mongosa.Configuration
{
    public static class HostConfiguration
    {
        public static IHost ConfigureMongosa(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var mongoDbPreparation = services.GetRequiredService<IMongoPreparation>();
            mongoDbPreparation.EnsureInitialized();

            return host;
        }

        public static IHost RunMongoMigrations(this IHost host, Assembly assembly)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var migrations = services.GetRequiredService<IMongoMigrations>();
            migrations.ExecuteAsync(assembly).Wait();

            return host;
        }
    }


}