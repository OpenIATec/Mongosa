using MongoDB.Driver;

namespace Mongosa.Interfaces
{
    /// <summary>
    /// Represents a class that can configure indexes for an entity
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    internal interface IMongoIndexConfiguration<T>
    {
        /// <summary>
        /// Configure indexes for the entity
        /// </summary>
        /// <param name="indexManager">Index manager</param>
        void ConfigureIndex(IMongoIndexManager<T> indexManager);
    }
}