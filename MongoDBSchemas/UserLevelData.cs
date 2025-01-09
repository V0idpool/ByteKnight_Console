using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    // User level data MongoDB Schema
    [BsonIgnoreExtraElements]
    public class UserLevelData
    {
        [BsonId]
        public ObjectId Id { get; set; }  // MongoDB's unique identifier
        public ulong ServerId { get; set; }  // Server ID
        public string Name { get; set; }
        public ulong ID { get; set; }  // User ID
        public string AvatarURL { get; set; }  // User Avatar
        public int XP { get; set; }  // XP points
        public int MessageCount { get; set; }  // Number of messages sent
        public string BackgroundImageUrl { get; set; } // Add this line
        public DateTime? LastVoteRewardTime { get; set; }
        public bool VoteReminder { get; set; }
        public DateTime? LastVoteReminderSent { get; set; }
        public bool StealReminder { get; set; }
        public DateTime? LastStealReminderSent { get; set; }
        public int StealCount { get; set; }
        public int StealTotal { get; set; }
        public int StolenFromCount { get; set; }
        public int StolenTotal { get; set; }
        public DateTime? LastStealTime { get; set; } // Add this line
        public DateTime? AutoStealTime { get; set; } // Add this line
        public DateTime? DoubleXpExpiry { get; set; }
        // Additional fields as needed
        public int Level => CalculateLevel();

        [BsonIgnore]
        public int XpForNextLevel => CalculateXpRequiredForLevel(Level + 1);  // XP required for next level

        public int CalculateLevel()
        {
            return (int)Math.Floor(0.2 * Math.Sqrt(XP));
        }

        public int CalculateXpRequiredForLevel(int level)
        {
            return (int)Math.Pow(level / 0.2, 2);
        }
    }
}
