using MongoDB.Driver;

namespace Mongosa.Interfaces
{
    public interface IMongoContext
    {
        IMongoDatabase Database { get; set; }
        IMongoClient MongoClient { get; set; }
        IMongoCollection<T> GetCollection<T>(string name);
    }
}