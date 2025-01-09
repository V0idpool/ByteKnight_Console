using ByteKnightConsole.Helpers;
using Discord;
using Discord.WebSocket;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Purge
    {
        private readonly ByteKnightEngine _botInstance;

        public Purge(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User as SocketGuildUser;

                var isAdmin = user?.GuildPermissions.Administrator ?? false;
                var hasSpecificRole = user?.GuildPermissions.ManageMessages ?? false;
                var isBot = user?.IsBot ?? false;

                if (isAdmin || hasSpecificRole || isBot)
                {
                    int messagesToPurge = (int)(long)slashCommand.Data.Options.First().Value;

                    // Optional filters
                    string filterUser = slashCommand.Data.Options.FirstOrDefault(x => x.Name == "user")?.Value as string;
                    string filterKeyword = slashCommand.Data.Options.FirstOrDefault(x => x.Name == "keyword")?.Value as string;
                    bool filterBots = (bool?)(slashCommand.Data.Options.FirstOrDefault(x => x.Name == "bots-only")?.Value) ?? false;

                    var channel = slashCommand.Channel as SocketTextChannel;

                    if (channel != null)
                    {
                        await slashCommand.DeferAsync(ephemeral: true);

                        try
                        {
                            var messages = await channel.GetMessagesAsync(messagesToPurge).FlattenAsync();

                            // Apply optional filters
                            if (!string.IsNullOrEmpty(filterUser))
                            {
                                messages = messages.Where(m => m.Author.Username.Equals(filterUser, StringComparison.OrdinalIgnoreCase));
                            }

                            if (!string.IsNullOrEmpty(filterKeyword))
                            {
                                messages = messages.Where(m => m.Content.Contains(filterKeyword, StringComparison.OrdinalIgnoreCase));
                            }

                            if (filterBots)
                            {
                                messages = messages.Where(m => m.Author.IsBot);
                            }

                            // Separate messages into those older and newer than 14 days
                            var oldMessages = messages.Where(m => (DateTimeOffset.Now - m.CreatedAt).TotalDays >= 14).ToList();
                            var recentMessages = messages.Where(m => (DateTimeOffset.Now - m.CreatedAt).TotalDays < 14).ToList();

                            // Max number of messages to delete per batch
                            const int batchSize = 100;
                            var batches = recentMessages.Batch(batchSize);

                            foreach (var batch in batches)
                            {
                                try
                                {
                                    await channel.DeleteMessagesAsync(batch);
                                    // Delay between batches to handle rate limits
                                    await Task.Delay(1000);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error deleting messages: {ex.Message}");
                                }
                            }

                            // Use DeleteOldMessagesAsync to handle older messages
                            await DeleteOldMessagesAsync(channel, oldMessages);

                            await slashCommand.FollowupAsync($"{user.GlobalName ?? user.Username} Purged {recentMessages.Count()} recent messages and {oldMessages.Count} old messages.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during purge operation: {ex.Message}");
                            await slashCommand.FollowupAsync("An error occurred while purging messages. Please try again later.", ephemeral: true);
                        }
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to use this command. (Command usable by Admins, and users with Manage Messages Permission. IE. Moderator Role)", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
        public async Task DeleteOldMessagesAsync(SocketTextChannel channel, IEnumerable<IMessage> oldMessages)
        {
            // Limit the number of concurrent deletions, avoids rate limit issues
            const int maxConcurrency = 5;
            var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = oldMessages.Select(async message =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await channel.DeleteMessageAsync(message);
                    // Delay to handle rate limits 5.5 seconds
                    await Task.Delay(5500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting message: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
