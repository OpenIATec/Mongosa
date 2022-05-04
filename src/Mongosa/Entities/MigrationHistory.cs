using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongosa.Entities
{
    public class MigrationHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string TimeStamp { get; private set; }
        public string Name { get; private set; }
        public DateTime AppliedAt { get; private set; }

        public MigrationHistory(string timeStamp, string name, DateTime appliedAt)
        {
            TimeStamp = timeStamp;
            Name = name;
            AppliedAt = appliedAt;
        }
    }
}