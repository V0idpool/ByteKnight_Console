using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class WarnClear
    {
        private readonly ByteKnightEngine _botInstance;

        public WarnClear(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var authorBan = slashCommand.User as SocketGuildUser;

                // Check if the author has the "Ban Members" or "Administrator" permission
                if (authorBan.GuildPermissions.Administrator || authorBan.GuildPermissions.BanMembers)
                {
                    var userIdOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "user");
                    var warnNumberOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "reason");  // Changed from "reason" to "number"

                    // Extract user ID from the option
                    ulong userId = userIdOption?.Value is SocketUser user ? user.Id : 0;
                    int warnNumber = int.TryParse(warnNumberOption?.Value?.ToString() ?? string.Empty, out var number) ? number : 0;

                    if (userId == 0 || warnNumber <= 0)
                    {
                        await slashCommand.RespondAsync("Please provide a valid user and a warning number greater than 0.", ephemeral: true);
                        return;
                    }

                    // Get the user instance from the ID
                    var userInstance = ByteKnightEngine._client.GetUser(userId);


                    ulong serverId = (slashCommand.Channel as ITextChannel)?.Guild?.Id ?? 0; // Get server ID from the channel

                    // Get the warnings for the user
                    var warnings = await Warn.GetWarnings(userId, serverId);

                    if (warnNumber <= warnings.Count)
                    {
                        // Get the specific warning based on the number (1-based index)
                        var warningToRemove = warnings[warnNumber - 1];
                        string reason = warningToRemove.Reason;  // Extract the reason for the warning
                        ulong issuerId = warningToRemove.IssuerId;  // Get the issuer's ID

                        // Remove the warning
                        await Warn.RemoveWarning(userId, serverId, warnNumber);

                        // Get the issuer's user instance
                        var issuer = ByteKnightEngine._client.GetUser(issuerId);

                        var embedBuilder = new EmbedBuilder
                        {
                            Title = $"✅ Warning Removed",
                            Color = Color.Green,
                            Description = $"Warning number {warnNumber} for {userInstance.Mention} has been removed.\n**Reason:** {reason}\n**Issued By:** {(issuer != null ? $"<@{issuer.Id}>" : "Unknown")}",
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "\u200B\n┤|ByteKnight Discord Bot|├\nhttps://voidbot.lol/",
                                IconUrl = "https://i.imgur.com/SejD45x.png",
                            },
                            Timestamp = DateTime.UtcNow,
                        };
                        var embed = embedBuilder.Build();
                        await slashCommand.RespondAsync(embed: embed, ephemeral: true);
                    }
                    else
                    {
                        var embedBuilder = new EmbedBuilder
                        {
                            Title = $"⚠️ Warning Not Found",
                            Color = Color.Red,
                            Description = $"No warning found for {userInstance.Mention} with number {warnNumber}.",
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "\u200B\n┤|ByteKnight Discord Bot|├\nhttps://voidbot.lol/",
                                IconUrl = "https://i.imgur.com/SejD45x.png",
                            },
                            Timestamp = DateTime.UtcNow,
                        };
                        var embed = embedBuilder.Build();
                        await slashCommand.RespondAsync(embed: embed, ephemeral: true);
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
