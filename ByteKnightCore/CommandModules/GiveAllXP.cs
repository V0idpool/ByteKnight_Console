using ByteKnightConsole.MongoDBSchemas;
using Discord.WebSocket;
using MongoDB.Bson;
using  ByteKnightConsole.ByteKnightCore;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class GiveAllXP
    {
        private readonly ByteKnightEngine _botInstance;

        public GiveAllXP(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User;
                var isBot = user?.IsBot ?? false;
                var amountOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "amount");
                var authorBan = slashCommand.User as SocketGuildUser;

                if (authorBan != null && (authorBan.GuildPermissions.Administrator || authorBan.GuildPermissions.BanMembers))
                {
                    if (amountOption?.Value is long amount)
                    {
                        await slashCommand.DeferAsync(ephemeral: false);

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                                var users = new List<SocketGuildUser>();

                                Console.WriteLine("Fetching users...");
                                // Collect all users from the guild asynchronously
                                await foreach (var userBatch in authorBan.Guild.GetUsersAsync())
                                {
                                    Console.WriteLine($"Processing batch of {userBatch.Count()} users...");
                                    // Filter out bots
                                    users.AddRange(userBatch.OfType<SocketGuildUser>().Where(user => !user.IsBot));
                                }

                                Console.WriteLine($"Total users fetched: {users.Count}");
                                Console.WriteLine("Awarding XP to users...");
                                // Adjust as needed
                                int batchSize = 50;
                                int totalUsers = users.Count;
                                for (int i = 0; i < totalUsers; i += batchSize)
                                {
                                    var batch = users.Skip(i).Take(batchSize).ToList();
                                    foreach (var user in batch)
                                    {
                                        var userId = user.Id;
                                        var userLevel = await MongoDBDriver.Helpers.LoadUserLevel(userId, serverId) ?? new UserLevelData
                                        {
                                            Id = ObjectId.GenerateNewId(),
                                            ID = userId,
                                            ServerId = serverId,
                                            Name = user.Username
                                        };

                                        int oldLevel = userLevel.Level;
                                        userLevel.XP += (int)amount;
                                        await MongoDBDriver.Helpers.SaveUserLevel(userLevel);

                                        int newLevel = userLevel.Level;
                                        if (newLevel > oldLevel)
                                        {
                                            Console.WriteLine($"Batch processing: Updating roles for user {user.Username} (ID: {userId}) from level {oldLevel} to level {newLevel}...");

                                        }
                                    }
                                    Console.WriteLine($"Processed batch {i / batchSize + 1} of {Math.Ceiling((double)totalUsers / batchSize)}");
                                    // Delay to prevent rate limiting
                                    await Task.Delay(1500);
                                }

                                Console.WriteLine("All users processed.");
                                await slashCommand.FollowupAsync($"All of you get... {amount} XP!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing giveallxp command: {ex.Message}");
                                await slashCommand.FollowupAsync("An error occurred while processing the giveallxp command.", ephemeral: true);
                            }
                        });
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Invalid command usage. Please specify an amount of XP.", ephemeral: true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
