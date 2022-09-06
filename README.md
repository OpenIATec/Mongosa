# Mongosa
MongoDB Simple Access

## Configuration

In Startup.cs
```csharp
services.AddMongosa(clientSettings =>
{
    clientSettings = MongoClientSettings.FromConnectionString("");
    clientSettings.ClusterConfigurator = builder => builder.Subscribe(new MongoDbEventSubscriber());
})

services.AddSingleton<MongoClientSettings>(sp => new new MongoClientSettings().FromConnectionString("")));
services.AddMongosa();
```

In Program.cs
```csharp
host
.ConfigureMongosa()
.RunMongoMigrations()
```

## Migration