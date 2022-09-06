using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Mongosa.Interfaces;

namespace Mongosa.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMongosa(this IServiceCollection services, Action<MongoClientSettings> mongoClientSettings = null, Action<DatabaseSettings> databaseSettings = null)
        {
            if (mongoClientSettings != null)
            {
                var opt = new MongoClientSettings();
                mongoClientSettings.Invoke(opt);
                services.AddSingleton(sp => opt);
            }
            else
            {
                services.AddSingleton(sp => sp.GetRequiredService<MongoClientSettings>());
            }
            
            if (databaseSettings != null)
            {
                var opt = new DatabaseSettings();
                databaseSettings.Invoke(opt);
                services.AddSingleton(sp => opt);
            }
            else
            {
                services.AddSingleton(sp => sp.GetRequiredService<DatabaseSettings>());
            }

            services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<MongoClientSettings>()));

            services.AddScoped<IMongoMigrations, MongoMigrations>();
            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton<MongoPreparation>();
            services.AddSingleton<IMongoPreparation>(sp => sp.GetRequiredService<MongoPreparation>());
            services.AddSingleton<IMongoCollectionNames>(sp => sp.GetRequiredService<MongoPreparation>());

            return services;
        }
    }
}