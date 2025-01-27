﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    /// <summary>
    /// Represents mute information for a user in a guild stored in MongoDB.
    /// </summary>
    public class MuteInfo
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public DateTime UnmuteTime { get; set; }
    }
}
