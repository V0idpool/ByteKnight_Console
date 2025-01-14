using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    public class Warning
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ServerId { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
        public ulong IssuerId { get; set; }
    }
}
