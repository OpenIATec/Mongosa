using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Mongosa.Interfaces;

namespace Mongosa.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMongosa(this IServiceCollection services, Action<DatabaseSettings> settings = null)
        {
            var opt = new DatabaseSettings();
            settings?.Invoke(opt);

            if (settings != null)
            {
                services.AddSingleton<DatabaseSettings>(sp => opt);
            }
            else
            {
                services.AddSingleton<DatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            }

            services.AddScoped<IMongoMigrations, MongoMigrations>();
            services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<DatabaseSettings>().ConnectionString));
            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton<MongoPreparation>();
            services.AddSingleton<IMongoPreparation>(sp => sp.GetRequiredService<MongoPreparation>());
            services.AddSingleton<IMongoCollectionNames>(sp => sp.GetRequiredService<MongoPreparation>());

            return services;
        }
    }
}