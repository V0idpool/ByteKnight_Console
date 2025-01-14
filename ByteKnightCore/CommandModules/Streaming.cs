using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Streaming
    {
        private readonly ByteKnightEngine _botInstance;

        public Streaming(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User as SocketGuildUser;

                ulong serverId = user.Guild.Id;

                var serverSettings = await MongoDBDriver.Helpers.GetServerSettings(serverId);
                if (serverSettings == null)
                {
                    await slashCommand.RespondAsync("Server settings not found.", ephemeral: true);
                    return;
                }
                ulong streamerRoleId = serverSettings.StreamerRole;
                var hasStreamerRole = user.Roles.Any(role => role.Id == streamerRoleId);
                bool isAdmin = user.GuildPermissions.Administrator;
                bool isOwner = user.Guild.OwnerId == user.Id;
                bool isModerator = user.GuildPermissions.KickMembers;

                if (isAdmin || isOwner || isModerator || hasStreamerRole)
                {
                    var twitchNameOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "twitch-name");
                    var gameNameOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "game-name");

                    if (twitchNameOption != null && gameNameOption != null)
                    {
                        string twitchName = twitchNameOption.Value.ToString();
                        string gameName = gameNameOption.Value.ToString();

                        DateTime now = DateTime.Now;
                        string displayName = user.DisplayName ?? user.Username;
                        string formattedDateTime = now.ToString("MMMM dd, yyyy" + Environment.NewLine + "'Time:' h:mm tt");

                        var embed = new EmbedBuilder
                        {
                            Title = " ❗ Stream Alert ❗",
                            Color = new Color(0, 255, 0),
                            Description = $"**{displayName}** is now **LIVE** on Twitch\n\n**Playing:**  _{gameName}_",
                            Timestamp = DateTime.UtcNow,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                                IconUrl = "https://i.imgur.com/SejD45x.png",
                            },
                            ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/twitchico.png",
                            Author = new EmbedAuthorBuilder
                            {
                                Name = $"{displayName} Just went LIVE!",
                                IconUrl = user.GetAvatarUrl()
                            },
                            Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Started At",
                        Value = formattedDateTime,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Watch Now",
                        Value = $"[Click Here to Watch](https://www.twitch.tv/{twitchName})",
                        IsInline = true
                    }
                }
                        };


                        await slashCommand.RespondAsync(embed: embed.Build());
                        Console.WriteLine("Twitch Update sent");
                    }
                    else
                    {
                        await slashCommand.RespondAsync("Please provide a Twitch username and game name. Usage: `/live twitchname gamename`", ephemeral: true);
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("You don't have permission to use this command.", ephemeral: true);
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
