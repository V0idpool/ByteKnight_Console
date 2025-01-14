using ByteKnightConsole.MongoDBSchemas;
using Discord.WebSocket;
using MongoDB.Bson;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class GiveXP
    {
        private readonly ByteKnightEngine _botInstance;

        public GiveXP(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var userOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user");
                var amountOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "amount");
                var authorBan = slashCommand.User as SocketGuildUser;

                if (authorBan.GuildPermissions.Administrator || authorBan.GuildPermissions.BanMembers)
                {
                    if (userOption?.Value is SocketGuildUser user && amountOption?.Value is long amount)
                    {
                        await slashCommand.DeferAsync(ephemeral: true);

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var userId = user.Id;
                                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;

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
                                await slashCommand.FollowupAsync($"{user.Mention} has been awarded {amount} XP. Total XP: {userLevel.XP}, Level: {userLevel.Level}", ephemeral: true);
                                int newLevel = userLevel.Level;

                                if (newLevel > oldLevel)
                                {
                                    //do a thing


                                }
                                else
                                {
                                    //do nothing, no role update required
                                    Console.WriteLine("XP Given, No Role Update required.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing givexp command: {ex.Message}");
                                await slashCommand.FollowupAsync("An error occurred while processing the givexp command.", ephemeral: true);
                            }
                        });
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Invalid command usage. Please specify a user and an amount of XP.", ephemeral: true);
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
