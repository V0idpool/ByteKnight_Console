using ByteKnightConsole.ByteKnightCore;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.Helpers
{
    /// <summary>
    /// Helps with purging messages from Discord channels.
    /// </summary>
    public class PurgeHelper
    {
        private readonly ByteKnightEngine _botInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="PurgeHelper"/> class.
        /// </summary>
        /// <param name="botInstance">The instance of the bot engine.</param>
        public PurgeHelper(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }
        /// <summary>
        /// Retrieves the ID of a Discord channel by its name.
        /// </summary>
        /// <param name="channelName">The name of the channel.</param>
        /// <returns>The channel ID, or 0 if not found.</returns>
        public async Task<ulong> GetChannelIdByName(string channelName)
        {

            var guild = (ByteKnightEngine.DiscordClient as DiscordSocketClient)?.Guilds.FirstOrDefault();

            if (guild != null)
            {
                var channels = guild.Channels;
                var channel = channels.FirstOrDefault(c => c.Name == channelName);
                return channel?.Id ?? 0;
            }

            return 0;
        }
        /// <summary>
        /// Initiates the purge process for a channel identified by name, deleting a specified number of messages.
        /// </summary>
        /// <param name="channelName">The name of the channel.</param>
        /// <param name="messagesToPurge">The number of messages to attempt to purge.</param>
        public async Task HandlePurgeForChannel(string channelName, int messagesToPurge)
        {
            ulong channelId = await GetChannelIdByName(channelName);
            if (channelId != 0)
            {
                await HandlePurgeForChannel(channelId, messagesToPurge);
            }
            else
            {
                Console.WriteLine($"Channel ID not found for the selected channel name: {channelName}", "Channel ID Error");
            }
        }
        /// <summary>
        /// Initiates the purge process for a channel identified by ID, deleting a specified number of recent messages.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="messagesToPurge">The number of messages to attempt to purge.</param>
        public async Task HandlePurgeForChannel(ulong channelId, int messagesToPurge)
        {
            var channel = ByteKnightEngine.DiscordClient.GetChannel(channelId) as ISocketMessageChannel;
            Random rnd = new Random();

            if (channel != null)
            {
                // Fetch messages and filter out those older than two months
                var messages = await channel.GetMessagesAsync(messagesToPurge).FlattenAsync();
                var messagesToDelete = messages.Where(m => (DateTimeOffset.Now - m.CreatedAt).TotalDays < 60);

                int deletedCount = 0;

                foreach (var message in messagesToDelete)
                {
                    // Add a random delay between 1 and 5 seconds before deleting each message to avoid rate limits
                    await Task.Delay(rnd.Next(1000, 5001));
                    await message.DeleteAsync();
                    deletedCount++;
                }
                await channel.SendMessageAsync($"Purged {deletedCount} messages.");
                Console.WriteLine($"Successfully purged {deletedCount} messages.");
            }
        }
    }
}
