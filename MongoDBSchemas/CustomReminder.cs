using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    public class CustomReminder
    {
        [BsonId]
        public ObjectId Id { get; set; }  // MongoDB internal ID
        public ulong ServerId { get; set; } // Server ID
        public string UserName { get; set; }
        public ulong UserId { get; set; }
        public string ReminderMessage { get; set; }
        public int TimerValue { get; set; }
        public DateTime TriggerTime { get; set; }
    }
}
