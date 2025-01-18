using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    [BsonIgnoreExtraElements]
    /// <summary>
    /// Represents server-specific settings stored in MongoDB.
    /// </summary>
    public class ServerSettings
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong WelcomeChannelId { get; set; }
        public ulong RulesChannelId { get; set; }
        public ulong WarnPingChannelId { get; set; }
        public ulong TicketPingChannelId { get; set; }
        public ulong LogPingChannelId { get; set; }
        public ulong QuarantineChannelID { get; set; }
        public ulong SecurityPingChannelID { get; set; }
        public ulong LevelChannelId { get; set; }
        public ulong StarboardChannelId { get; set; }
        public int StarboardReactionThreshhold { get; set; }
        public int SpamCount { get; set; }
        public int SpamTimeFrame { get; set; }
        public ulong AiHomeChannel { get; set; }
        public ulong AutoRoleId { get; set; }
        public ulong StreamerRole { get; set; }
        public Dictionary<string, ulong> RolePings { get; set; } = new Dictionary<string, ulong>(); // Use Dictionary
        public string WelcomeMessage { get; set; }
        public string YoutubeURL { get; set; }
        public string TwitchURL { get; set; }
        public string SteamURL { get; set; }
        public string FacebookURL { get; set; }
        public string InviteURL { get; set; }
        public bool SetupNotificationSent { get; set; } = false;
        public string Prefix { get; set; }
        public int XPCooldown { get; set; }
        public int XPAmount { get; set; }
        public bool XPSystemEnabled { get; set; }
        public bool VoiceXPEnabled { get; set; }
        public int VoiceXPAmount { get; set; }
        public DateTime LastTheftReset { get; set; }
    }
}
