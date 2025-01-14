using ByteKnightConsole.MongoDBSchemas;
using Discord.WebSocket;
using MongoDB.Driver;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class UnMute
    {
        private readonly ByteKnightEngine _botInstance;

        public UnMute(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var userOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user");

                if (userOption?.Value is SocketGuildUser user)
                {
                    var authorUnmute = slashCommand.User as SocketGuildUser;

                    if (authorUnmute.GuildPermissions.Administrator || authorUnmute.GuildPermissions.BanMembers)
                    {
                        var botUser = authorUnmute.Guild.GetUser(ByteKnightEngine._client.CurrentUser.Id);

                        if (!botUser.GuildPermissions.ManageRoles)
                        {
                            await slashCommand.RespondAsync("I do not have permission to manage roles.", ephemeral: true);
                            return;
                        }

                        var muteRole = authorUnmute.Guild.Roles.FirstOrDefault(role => role.Name == "Muted");

                        if (muteRole != null)
                        {
                            if (botUser.Hierarchy <= muteRole.Position)
                            {
                                await slashCommand.RespondAsync("My role is not high enough to remove the 'Muted' role.", ephemeral: true);
                                return;
                            }

                            await slashCommand.DeferAsync(ephemeral: true);

                            // Perform the role removal and database operation in a separate task
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    // Remove mute entry from MongoDB
                                    var filter = Builders<MuteInfo>.Filter.Eq("UserId", user.Id) & Builders<MuteInfo>.Filter.Eq("GuildId", authorUnmute.Guild.Id);
                                    var result = await ByteKnightEngine._muteCollection.FindOneAndDeleteAsync(filter);

                                    if (result != null)
                                    {
                                        await user.RemoveRoleAsync(muteRole);

                                        await slashCommand.FollowupAsync($"{user.Mention} has been unmuted.");
                                    }
                                    else
                                    {
                                        await slashCommand.FollowupAsync("User was not muted.", ephemeral: true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                    await slashCommand.FollowupAsync("An error occurred while unmuting the user.", ephemeral: true);
                                }
                            });
                        }
                        else
                        {
                            await slashCommand.RespondAsync("Muted role not found.", ephemeral: true);
                        }
                    }
                    else
                    {
                        await slashCommand.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("Invalid command usage. Please specify a user.", ephemeral: true);
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
