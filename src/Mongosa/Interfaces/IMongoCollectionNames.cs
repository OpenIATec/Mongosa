using System;

namespace Mongosa.Interfaces
{
    /// <summary>
    /// Represents a class that provides the name for typed collections
    /// </summary>
    public interface IMongoCollectionNames
    {
        /// <summary>
        /// Get the collection name for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Entity type</param>
        /// <returns>Collection name</returns>
        string CollectionNameForType(Type type);
    }
}