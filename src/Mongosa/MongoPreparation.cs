using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Mongosa.Attributes;
using Mongosa.Interfaces;

namespace Mongosa
{
    /// <summary>
    /// A preparation class for MongoDb for registering conventions, mappings and indexes
    /// </summary>
    public class MongoPreparation : IMongoPreparation, IMongoCollectionNames
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly Assembly assembly;
        private readonly Dictionary<Type, string> collectionNames = new Dictionary<Type, string>();

        /// <summary>
        /// Indicate if this preparation was executed
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Create a new instance of the preparation
        /// </summary>
        /// <param name="serviceScopeFactory">Application's service scope factory</param>
        public MongoPreparation(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            assembly = GetType().Assembly;
        }

        /// <inheritdoc />
        public void EnsureInitialized()
        {
            lock (assembly)
            {
                if (IsInitialized)
                    return;

                RegisterConventions();

                RegisterMappings();

                RegisterIndexes();

                IsInitialized = true;
            }
        }

        /// <inheritdoc />
        public string CollectionNameForType(Type type)
        {
            var name = GetCollectionName(type);

            if (!collectionNames.ContainsKey(type))
                collectionNames.Add(type, name);

            return collectionNames[type];
        }

        private static string GetCollectionName<T>(T type)
        {
            var attribute = ((type as Type)?.GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault() as BsonCollectionAttribute);
            return attribute != null ? attribute.CollectionName : (type as Type)?.Name;
        }

        /// <summary>
        /// Register all conventions
        /// </summary>
        private void RegisterConventions()
        {
            var conventionBase = typeof(IMemberMapConvention);
            var conventionTypes = assembly.GetTypes().Where(t => conventionBase.IsAssignableFrom(t));

            var pack = new ConventionPack();

            foreach (var conventionType in conventionTypes)
            {
                var convention = (IConvention)Activator.CreateInstance(conventionType);
                pack.Add(convention);
            }

            //pack.Add(new CamelCaseElementNameConvention());
            //pack.Add(new StringIdStoredAsObjectIdConvention());
            pack.Add(new IgnoreExtraElementsConvention(true));
            //pack.Add(new IgnoreIfDefaultConvention(true));
            //pack.Add(new EnumRepresentationConvention(BsonType.String));
            pack.Add(new LookupIdGeneratorConvention());

            ConventionRegistry.Register($"{GetType().Name} Conventions", pack, t => true);
        }

        /// <summary>
        /// Register all mappings
        /// </summary>
        private void RegisterMappings()
        {
            var mappingBase = typeof(BsonClassMap);
            var mappingTypes = new List<Type>();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(ass => !ass.FullName.Contains("MongoDB.Bson")))
            {
                mappingTypes.AddRange(a.GetTypes().Where(t => mappingBase.IsAssignableFrom(t) && !t.ContainsGenericParameters));
            }

            var collectionTypes = new List<Type>();

            foreach (var mappingType in mappingTypes)
            {
                var mapping = (BsonClassMap)Activator.CreateInstance(mappingType);
                collectionTypes.Add(mapping!.ClassType);
                if (!BsonClassMap.IsClassMapRegistered(mappingType))
                {
                    BsonClassMap.RegisterClassMap(mapping);
                }
            }

            foreach (var type in collectionTypes)
            {
                var t = type;
                while (t!.BaseType != null && collectionTypes.Contains(t.BaseType))
                {
                    t = type.BaseType;
                }
                collectionNames.Add(type, t.Name);
            }
        }

        /// <summary>
        /// Register all indexes
        /// </summary>
        private void RegisterIndexes()
        {
            var genericType = typeof(IMongoIndexConfiguration<>);
            var configurationTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType));

            using var scope = serviceScopeFactory.CreateScope();

            var mongoContext = scope.ServiceProvider.GetService(typeof(IMongoContext));

            var getCollectionGenericMethod = mongoContext.GetType().GetMethod(nameof(IMongoContext.GetCollection));

            foreach (var configurationType in configurationTypes)
            {
                var instance = Activator.CreateInstance(configurationType);

                var definitions = configurationType.GetInterfaces()
                    .Where(a => a.IsGenericType && a.GetGenericTypeDefinition() == genericType)
                    .ToArray();

                foreach (var interfaceType in definitions)
                {
                    var entityType = interfaceType.GetGenericArguments()[0];

                    var getCollectionMethod = getCollectionGenericMethod?.MakeGenericMethod(entityType);
                    var collection = getCollectionMethod?.Invoke(mongoContext, new object[] { CollectionNameForType(entityType) });

                    var propertyInfo = collection?.GetType().GetProperty("Indexes");
                    var indexManager = propertyInfo?.GetValue(collection);

                    var configureIndexMethod = interfaceType.GetMethod("ConfigureIndex");
                    configureIndexMethod?.Invoke(instance, new[] { indexManager });
                }
            }
        }
    }
}