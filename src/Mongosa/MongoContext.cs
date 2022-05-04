using System;
using MongoDB.Driver;
using Mongosa.Configuration;
using Mongosa.Interfaces;

namespace Mongosa
{
    public class MongoContext : IMongoContext
    {
        public IMongoDatabase Database { get; set; }
        public IMongoClient MongoClient { get; set; }

        public MongoContext(DatabaseSettings settings, IMongoClient mongoClient)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            MongoClient = mongoClient;
            Database = MongoClient.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            IMongoCollection<T> collection = Database.GetCollection<T>(name);
            return collection;
        }
    }
}