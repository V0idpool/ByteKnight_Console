using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    public class VerificationContexts
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public bool HasVerified { get; set; }
        public ulong WelcomeMessageId { get; set; }
        public ulong PingMessageId { get; set; }
    }

    public static class UserContextStore
    {
        private static IMongoCollection<VerificationContexts> _userContextCollection;
        /// <summary>
        /// Initializes the UserContextStore with the given MongoDB database.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        public static void Initialize(IMongoDatabase database)
        {
            _userContextCollection = database.GetCollection<VerificationContexts>("UserContexts");
        }

        /// <summary>
        /// Adds or updates a user context in the database.
        /// </summary>
        /// <param name="context">The user context to add or update.</param>
        public static async Task AddOrUpdateAsync(VerificationContexts context)
        {
            var filter = Builders<VerificationContexts>.Filter.And(
                Builders<VerificationContexts>.Filter.Eq(u => u.ServerId, context.ServerId),
                Builders<VerificationContexts>.Filter.Eq(u => u.UserId, context.UserId)
            );

            await _userContextCollection.ReplaceOneAsync(
                filter,
                context,
                new ReplaceOptions { IsUpsert = true }
            );
        }

        /// <summary>
        /// Retrieves a user context based on server and user IDs.
        /// </summary>
        /// <param name="serverId">The server's unique identifier.</param>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>The user context if found; otherwise, null.</returns>
        public static async Task<VerificationContexts> GetAsync(ulong serverId, ulong userId)
        {
            var filter = Builders<VerificationContexts>.Filter.And(
                Builders<VerificationContexts>.Filter.Eq(u => u.ServerId, serverId),
                Builders<VerificationContexts>.Filter.Eq(u => u.UserId, userId)
            );

            return await _userContextCollection.Find(filter).FirstOrDefaultAsync();
        }
        /// <summary>
        /// Deletes a user context from the database.
        /// </summary>
        /// <param name="serverId">The server's unique identifier.</param>
        /// <param name="userId">The user's unique identifier.</param>
        public static async Task DeleteAsync(ulong serverId, ulong userId)
        {
            var filter = Builders<VerificationContexts>.Filter.And(
                Builders<VerificationContexts>.Filter.Eq(u => u.ServerId, serverId),
                Builders<VerificationContexts>.Filter.Eq(u => u.UserId, userId)
            );

            await _userContextCollection.DeleteOneAsync(filter);
        }
    }
}
