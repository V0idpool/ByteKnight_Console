using Discord.WebSocket;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class RemoveXP
    {
        private readonly ByteKnightEngine _botInstance;

        public RemoveXP(ByteKnightEngine botInstance)
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

                                var userLevel = await MongoDBDriver.Helpers.LoadUserLevel(userId, serverId);
                                if (userLevel == null)
                                {
                                    await slashCommand.FollowupAsync("User does not have any XP.", ephemeral: true);
                                    return;
                                }

                                int oldLevel = userLevel.Level;
                                userLevel.XP -= (int)amount;
                                if (userLevel.XP < 0) userLevel.XP = 0;
                                await MongoDBDriver.Helpers.SaveUserLevel(userLevel);

                                int newLevel = userLevel.Level;
                                if (newLevel < oldLevel)
                                {
                                    await slashCommand.FollowupAsync($"{user.Mention}, you have been demoted to level {newLevel}.", ephemeral: true);

                                }
                                else
                                {
                                    await slashCommand.FollowupAsync($"{user.Mention} has lost {amount} XP. Total XP: {userLevel.XP}, Level: {userLevel.Level}", ephemeral: true);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing removexp command: {ex.Message}");
                                await slashCommand.FollowupAsync("An error occurred while processing the removexp command.", ephemeral: true);
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
