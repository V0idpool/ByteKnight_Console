using ByteKnightConsole;
using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Verify
    {
        private readonly ByteKnightEngine _botInstance;

        public Verify(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = (SocketGuildUser)slashCommand.User;

                if (user.GuildPermissions.Administrator || user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers)
                {
                    var userOption = (SocketGuildUser)slashCommand.Data.Options.FirstOrDefault()?.Value;

                    if (userOption == null)
                    {
                        await slashCommand.RespondAsync("You must specify a user to verify.", ephemeral: true);
                        return;
                    }

                    var guildUser = userOption as SocketGuildUser;
                    if (guildUser == null)
                    {
                        await slashCommand.RespondAsync("Could not find the specified user.", ephemeral: true);
                        return;
                    }

                    var serverId = slashCommand.GuildId.Value;
                    var serverSettings = await ByteKnightConsole.MongoDBDriver.Helpers.GetServerSettings(serverId);

                    if (serverSettings == null)
                    {
                        await slashCommand.RespondAsync("Server settings not found. Please contact support. [ByteKnight Support Server](https://discord.gg/trm9qEzcuw)", ephemeral: true);
                        return;
                    }

                    // Verification Auto role if the server has AutoRoleId configured (this role is given after the user verifies in the verification & welcome channel)
                    if (serverSettings.AutoRoleId != 0)
                    {
                        await guildUser.AddRoleAsync(serverSettings.AutoRoleId);

                        var successEmbed = new EmbedBuilder()
                            .WithTitle("✅ User Verified 🎉")
                            .WithDescription($"{guildUser.Username} has been manually verified and given access to the server.")
                            .WithColor(new Color(0, 204, 102))
                            .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/refs/heads/main/Img/unlocked.png")
                            .WithFooter(footer =>
                            {
                                footer.Text = "Manual verification complete.";
                                footer.IconUrl = "https://i.imgur.com/SejD45x.png";
                            })
                            .WithTimestamp(DateTime.UtcNow)
                            .Build();
                        await slashCommand.RespondAsync(embed: successEmbed, ephemeral: true);
                        var guild = ByteKnightEngine._client.GetGuild(serverId);
                        var serverName = guild.Name;
                        // Send DM to the verified user
                        var dmEmbed = new EmbedBuilder()
                            .WithTitle("✅ You Have Been Verified! 🎉")
                            .WithDescription($"Your account has been manually verified by an admin, and you've been granted access to **{serverName}**. Welcome!")
                            .WithColor(new Color(0, 204, 102))
                            .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/refs/heads/main/Img/unlocked.png")
                            .WithFooter(footer =>
                            {
                                footer.Text = "Verified by the admin team.";
                                footer.IconUrl = "https://i.imgur.com/SejD45x.png";
                            })
                            .WithTimestamp(DateTime.UtcNow)
                            .Build();

                        try
                        {
                            await guildUser.SendMessageAsync(embed: dmEmbed);
                        }
                        catch (Exception)
                        {
                            await slashCommand.FollowupAsync($"Failed to send DM to {guildUser.Username}. They might have DMs disabled.", ephemeral: true);
                        }

                        return;
                    }
                    else
                    {
                        await slashCommand.RespondAsync("AutoRole is not configured. Please contact support.", ephemeral: true);
                        return;
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("You do not have permission to use this command. This action requires Admin, Kick, or Ban permissions.", ephemeral: true);
                    return;
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
