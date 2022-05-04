namespace Mongosa.Interfaces
{
    /// <summary>
    /// Represents a class capable of preparing MongoDb for usage
    /// </summary>
    public interface IMongoPreparation
    {
        /// <summary>
        /// Indicate if this preparation was executed
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Perform the initialization process
        /// </summary>
        void EnsureInitialized();
    }
}