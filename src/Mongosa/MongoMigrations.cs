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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMongoCollection<MigrationHistory> _migrationHistoryContext;
        public MongoMigrations(IServiceScopeFactory scopeFactory, IMongoContext context, IMongoCollectionNames collectionNames)
        {
            this._scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            if (collectionNames == null)
            {
                throw new ArgumentNullException(nameof(collectionNames));
            }

            _migrationHistoryContext = context.GetCollection<MigrationHistory>(collectionNames.CollectionNameForType(typeof(MigrationHistory)));
        }


        public async Task ExecuteAsync(Assembly assembly)
        {
            var query = _migrationHistoryContext
                .Find(m => true)
                .SortByDescending(m => m.TimeStamp);

            var lastAppliedMigration = query.FirstOrDefault();

            var migrations = assembly
                ?.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Migration)))
                .Select(type => (Migration)Activator.CreateInstance(type, _scopeFactory))
                .Where(migration => migration != null && string.Compare(migration.TimeStamp, lastAppliedMigration == null ? "0" : lastAppliedMigration.TimeStamp, StringComparison.InvariantCulture) == 1)
                .OrderBy(migration => migration.TimeStamp);

            foreach (var migration in migrations)
            {
                try
                {
                    migration.Up();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                var history = new MigrationHistory(migration.TimeStamp, migration.GetType().Name, DateTime.Now);
                await _migrationHistoryContext.InsertOneAsync(history);
            }
        }
    }
}