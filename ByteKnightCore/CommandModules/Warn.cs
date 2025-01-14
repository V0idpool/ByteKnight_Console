using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using Color = Discord.Color;
namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Warn
    {
        private readonly ByteKnightEngine _botInstance;

        public Warn(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var author = slashCommand.User as SocketGuildUser;

                if (author?.GuildPermissions.Administrator == true || author.GuildPermissions.BanMembers == true || author.GuildPermissions.KickMembers)
                {
                    var userIdOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "user");
                    var reasonOption = slashCommand.Data.Options.FirstOrDefault(o => o.Name == "reason");
                    var user = userIdOption?.Value as SocketUser;
                    string reason = reasonOption?.Value?.ToString() ?? string.Empty;

                    if (user == null || string.IsNullOrWhiteSpace(reason))
                    {
                        await slashCommand.RespondAsync("Please provide a valid user and a reason for the warning.", ephemeral: true);
                        return;
                    }

                    ulong serverId = (slashCommand.Channel as SocketGuildChannel)?.Guild.Id ?? 0;
                    if (serverId == 0)
                    {
                        await slashCommand.RespondAsync("Unable to determine the server ID.", ephemeral: true);
                        return;
                    }
                    await AddWarning(user.Id, slashCommand.User.Id, serverId, reason);
                    var userWarnings = await GetWarnings(user.Id, serverId);
                    int totalWarnings = userWarnings.Count;

                    DateTime? lastWarningDate = userWarnings.OrderByDescending(w => w.Date).FirstOrDefault()?.Date;

                    var serverSettings = await MongoDBDriver.Helpers.GetServerSettings(serverId);
                    // Send ping to the warnpingchannelid after the warning is issued
                    if (serverSettings != null && serverSettings.WarnPingChannelId != 0)
                    {
                        var guild = (slashCommand.Channel as SocketGuildChannel)?.Guild;
                        var warnChannel = guild?.GetTextChannel(serverSettings.WarnPingChannelId);

                        if (warnChannel != null)
                        {
                            var confirmationEmbed = new EmbedBuilder
                            {
                                Title = $"✅ Warning Issued",
                                Description = $"User {user.Mention} has been warned for the following:\n**Reason:** {reason}",
                                Color = Color.Green,
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                                    IconUrl = "https://i.imgur.com/SejD45x.png"
                                },
                                Timestamp = DateTime.UtcNow
                            };

                            await slashCommand.RespondAsync(embed: confirmationEmbed.Build(), ephemeral: true);

                            // Embed to notify roles and the server owner about the warning
                            var notifyEmbed = new EmbedBuilder
                            {
                                Title = "🚨 Warning Notification",
                                Color = Color.Red,
                                Description = $"A warning has been issued by: {slashCommand.User.Mention}",
                                Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Warned User",
                            Value = user.Mention,
                            IsInline = true
                        },
                        new EmbedFieldBuilder
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = true
                        },
        new EmbedFieldBuilder
        {
            Name = "\u200B",
            Value = "\u200B",
            IsInline = true
        },
                         new EmbedFieldBuilder
                {
                    Name = "Total Warnings",
                    Value = totalWarnings.ToString(),
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Last Warning Date",
                    Value = lastWarningDate.HasValue ? lastWarningDate.Value.ToString("g") : "No previous warnings",
                    IsInline = true
                }
                    },
                                Footer = new EmbedFooterBuilder
                                {
                                    Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                                    IconUrl = "https://i.imgur.com/SejD45x.png"
                                },
                                Timestamp = DateTime.UtcNow
                            };

                            await warnChannel.SendMessageAsync(embed: notifyEmbed.Build());

                            // Notify roles with permissions and the server owner
                            var rolesWithPermissions = guild.Roles
                                .Where(r => r.Permissions.BanMembers || r.Permissions.ManageGuild || r.Permissions.KickMembers)
                                .ToList();
                            var owner = guild.Owner;

                            var rolesMentions = string.Join(", ", rolesWithPermissions.Select(r => r.Mention));
                            if (string.IsNullOrEmpty(rolesMentions))
                            {
                                rolesMentions = "No roles with appropriate permissions found.";
                            }

                            await warnChannel.SendMessageAsync($"{rolesMentions}, {owner.Mention}");
                        }
                        else
                        {
                            await slashCommand.RespondAsync(
                                "Warning channel not found or invalid channel ID. Please visit [ByteKnight Dashboard](https://voidbot.lol/) to configure the bot.",
                                ephemeral: true
                            );
                        }
                    }
                    else
                    {
                        await slashCommand.RespondAsync(
                            "Warning channel is not configured for this server. Please visit [ByteKnight Dashboard](https://voidbot.lol/) to configure the bot.",
                            ephemeral: true
                        );
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("You do not have the required permissions to issue warnings.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
        // Warnings cleanup task
        public static async Task InitializeWarningCleanup()
        {
            await SetupWarningCleanup();

        }
        // Warning system below
        public static async Task AddWarning(ulong userId, ulong issuerId, ulong serverId, string reason)
        {
            var warning = new Warning
            {
                UserId = userId,
                IssuerId = issuerId,
                ServerId = serverId,
                Reason = reason,
                Date = DateTime.UtcNow
            };

            await ByteKnightEngine._warningsCollection.InsertOneAsync(warning);
        }


        public static async Task<List<Warning>> GetWarnings(ulong userId, ulong serverId)
        {
            var filter = Builders<Warning>.Filter.Eq(w => w.UserId, userId) &
                         Builders<Warning>.Filter.Eq(w => w.ServerId, serverId);
            var warnings = await ByteKnightEngine._warningsCollection.Find(filter).ToListAsync();
            return warnings.Where(w => DateTime.UtcNow.Subtract(w.Date).TotalDays <= 30).ToList();
        }


        public static async Task<bool> RemoveWarning(ulong userId, ulong serverId, int warningNumber)
        {
            var filter = Builders<Warning>.Filter.Eq(w => w.UserId, userId) &
                         Builders<Warning>.Filter.Eq(w => w.ServerId, serverId);
            var warnings = await ByteKnightEngine._warningsCollection.Find(filter).ToListAsync();
            var warningsToRemove = warnings.Where(w => DateTime.UtcNow.Subtract(w.Date).TotalDays <= 30).ToList();

            if (warningNumber <= warningsToRemove.Count)
            {
                var warningToRemove = warningsToRemove[warningNumber - 1];
                var deleteFilter = Builders<Warning>.Filter.Eq(w => w.Id, warningToRemove.Id);
                await ByteKnightEngine._warningsCollection.DeleteOneAsync(deleteFilter);
                return true;
            }

            return false;
        }


        public static async Task ClearWarnings(ulong userId, ulong serverId)
        {
            var filter = Builders<Warning>.Filter.Eq(w => w.UserId, userId) &
                         Builders<Warning>.Filter.Eq(w => w.ServerId, serverId);
            await ByteKnightEngine._warningsCollection.DeleteManyAsync(filter);
        }


        public static async Task RemoveOldWarnings()
        {
            if (ByteKnightEngine._warningsCollection == null)
            {
                Console.WriteLine("Warning collection is not initialized.");
                return;
            }
            var filter = Builders<Warning>.Filter.Lt(w => w.Date, DateTime.UtcNow.AddDays(-30));
            await ByteKnightEngine._warningsCollection.DeleteManyAsync(filter);
        }
        // Warning cleanup Task
        public static async Task SetupWarningCleanup()
        {
            if (ByteKnightEngine._warningsCollection == null)
            {
                Console.WriteLine("Warning collection is not initialized.");
                return;
            }
            var timer = new System.Threading.Timer(async _ => await RemoveOldWarnings(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
        }
    }
}
