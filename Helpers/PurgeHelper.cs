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
    public class PurgeHelper
    {
        private readonly ByteKnightEngine _botInstance;

        // Constructor accepts the bot engine and the GUI form references
        public PurgeHelper(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }
        // helper method to get the channels id by its name
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
