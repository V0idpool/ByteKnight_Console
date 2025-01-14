using Discord;
using Discord.WebSocket;
using ByteKnightConsole.MongoDBSchemas;
using MongoDB.Bson;
using MongoDB.Driver;


namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Mute
    {
        private readonly ByteKnightEngine _botInstance;

        public Mute(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var userOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "user");
                var durationOption = slashCommand.Data.Options.FirstOrDefault(option => option.Name == "duration");

                if (userOption?.Value is SocketGuildUser user && durationOption?.Value is long duration)
                {
                    var authorMute = slashCommand.User as SocketGuildUser;

                    if (authorMute.GuildPermissions.Administrator || authorMute.GuildPermissions.BanMembers)
                    {
                        var botUser = authorMute.Guild.GetUser(ByteKnightEngine._client.CurrentUser.Id);

                        if (!botUser.GuildPermissions.ManageRoles)
                        {
                            await slashCommand.RespondAsync("I do not have permission to manage roles.", ephemeral: true);
                            return;
                        }
                        var muteRole = authorMute.Guild.Roles.FirstOrDefault(role => role.Name == "Muted");
                        if (muteRole == null)
                        {
                            var restRole = await authorMute.Guild.CreateRoleAsync("Muted", new GuildPermissions(), isMentionable: false);
                            muteRole = authorMute.Guild.Roles.FirstOrDefault(role => role.Id == restRole.Id);

                            // Ensure "Muted" role has correct permissions in all channels
                            foreach (var channel in authorMute.Guild.Channels)
                            {
                                await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(sendMessages: PermValue.Deny));
                            }
                        }

                        if (muteRole != null)
                        {
                            if (botUser.Hierarchy <= muteRole.Position)
                            {
                                await slashCommand.RespondAsync("My role is not high enough to assign the 'Muted' role.", ephemeral: true);
                                return;
                            }

                            if (botUser.Hierarchy <= user.Hierarchy)
                            {
                                await slashCommand.RespondAsync("My role is not high enough to mute this user.", ephemeral: true);
                                return;
                            }

                            await user.AddRoleAsync(muteRole);

                            var unmuteTime = DateTime.UtcNow.AddMinutes(duration);
                            await slashCommand.RespondAsync($"{user.Mention} has been muted for {duration} minutes.", ephemeral: true);

                            var muteInfo = new MuteInfo
                            {
                                UserId = user.Id,
                                GuildId = authorMute.Guild.Id,
                                RoleId = muteRole.Id,
                                UnmuteTime = unmuteTime
                            };

                            await SaveMuteAsync(muteInfo);
                            await ScheduleUnmute(muteInfo);
                        }
                    }
                    else
                    {
                        await slashCommand.RespondAsync("You don't have permission to use this command.", ephemeral: true);
                    }
                }
                else
                {
                    await slashCommand.RespondAsync("Invalid command usage. Please specify a user and duration.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
        // Load mutes from MongoDB
        public static async Task<List<MuteInfo>> LoadMutesAsync()
        {
            var filter = Builders<MuteInfo>.Filter.Empty;
            var mutes = await ByteKnightEngine._muteCollection.Find(filter).ToListAsync();
            return mutes;
        }

        // Save mute to MongoDB
        public static async Task SaveMuteAsync(MuteInfo muteInfo)
        {
            await ByteKnightEngine._muteCollection.InsertOneAsync(muteInfo);
        }

        // Remove mute from MongoDB
        public static async Task RemoveMuteAsync(ObjectId muteId)
        {
            var filter = Builders<MuteInfo>.Filter.Eq("_id", muteId);
            await ByteKnightEngine._muteCollection.DeleteOneAsync(filter);
        }

        // Load and schedule mutes
        public static async Task LoadAndScheduleMutesAsync()
        {
            if (ByteKnightEngine._muteCollection == null)
            {
                Console.WriteLine("Warning collection is not initialized.");
                return;
            }
            var mutes = await LoadMutesAsync();
            foreach (var mute in mutes)
            {
                await ScheduleUnmute(mute);
            }
        }

        // Schedule unmute operation
        public static async Task ScheduleUnmute(MuteInfo muteInfo)
        {
            var delay = muteInfo.UnmuteTime - DateTime.UtcNow;
            if (delay.TotalMilliseconds <= 0)
            {
                // If delay is negative, set it to zero to unmute
                delay = TimeSpan.Zero;
            }

            var timer = new System.Timers.Timer(delay.TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await HandleUnmute(muteInfo, timer);
            // Ensure it only runs once
            timer.AutoReset = false;
            timer.Start();
        }
       
        // Method to unmute the user
        public static async Task HandleUnmute(MuteInfo muteInfo, System.Timers.Timer timer)
        {
            timer.Stop();
            timer.Dispose();

            var guild = ByteKnightEngine._client.GetGuild(muteInfo.GuildId);
            var user = guild?.GetUser(muteInfo.UserId);
            var muteRole = guild?.GetRole(muteInfo.RoleId);

            if (user != null && muteRole != null)
            {
                await user.RemoveRoleAsync(muteRole);

            }

            try
            {
                await user.SendMessageAsync($"{user.Mention}, you have been un-muted!");
            }
            catch
            {
                Console.WriteLine("Mute System: Un-mute message failed to send!");
            }
            // Remove mute entry in MongoDB
            await RemoveMuteAsync(muteInfo.Id);
        }
    }
}
