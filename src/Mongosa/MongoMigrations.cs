using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Mongosa.Entities;
using Mongosa.Interfaces;

namespace Mongosa
{
    /// <summary>
    /// Seeder class for the mongo database
    /// </summary>
    public class MongoMigrations : IMongoMigrations
    {
        private readonly IServiceScopeFactory scopeFactory;
        // private readonly IMongoCollection<Migration> migrationContext;
        private readonly IMongoCollection<MigrationHistory> migrationHistoryContext;
        public MongoMigrations(IServiceScopeFactory scopeFactory, IMongoContext context, IMongoCollectionNames collectionNames)
        {
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            if (collectionNames == null)
            {
                throw new ArgumentNullException(nameof(collectionNames));
            }

            // migrationContext = context.GetCollection<Migration>(collectionNames.CollectionNameForType(typeof(Migration)));
            migrationHistoryContext = context.GetCollection<MigrationHistory>(collectionNames.CollectionNameForType(typeof(MigrationHistory)));
        }


        public async Task ExecuteAsync(Assembly assembly)
        {
            var query = migrationHistoryContext
                .Find(m => true)
                .SortByDescending(m => m.TimeStamp);

            var lastAppliedMigration = query.FirstOrDefault();

            var migrations = assembly
                ?.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Migration)))
                .Select(type => (Migration)Activator.CreateInstance(type, scopeFactory))
                .Where(migration => migration != null && string.Compare(migration.TimeStamp, lastAppliedMigration == null ? "0" : lastAppliedMigration.TimeStamp, StringComparison.InvariantCulture) == 1)
                .OrderBy(migration => migration.TimeStamp);

            foreach (var migration in migrations)
            {
                migration.Up();
                var history = new MigrationHistory(migration.TimeStamp, migration.GetType().Name, DateTime.Now);
                await migrationHistoryContext.InsertOneAsync(history);
            }
        }
    }
}