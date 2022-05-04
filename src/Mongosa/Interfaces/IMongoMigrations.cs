using System.Reflection;
using System.Threading.Tasks;

namespace Mongosa.Interfaces
{
    /// <summary>
    /// Represents a class that seeds the mongo database
    /// </summary>
    public interface IMongoMigrations
    {
        /// <summary>
        /// Apply seed
        /// </summary>
        Task ExecuteAsync(Assembly assembly);
    }
}