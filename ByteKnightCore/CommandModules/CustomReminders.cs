using ByteKnightConsole.MongoDBSchemas;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class CustomReminders
    {
        private readonly ByteKnightEngine _botInstance;

        public CustomReminders(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User;
                var time = int.Parse(slashCommand.Data.Options.First(o => o.Name == "time").Value.ToString());
                var message = slashCommand.Data.Options.First(o => o.Name == "message").Value.ToString();
                var triggerTime = DateTime.UtcNow.AddMinutes(time);
                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                var reminder = new CustomReminder
                {
                    ServerId = serverId,
                    UserName = user.Username,
                    UserId = user.Id,
                    ReminderMessage = message,
                    TimerValue = time,
                    TriggerTime = triggerTime
                };

                // Save the reminder in the database
                await ByteKnightEngine._reminderCollection.InsertOneAsync(reminder);

                var embed = new EmbedBuilder
                {
                    Title = "⏰ Reminder Set",
                    Description = $"Your reminder has been set for **{time} minutes** from now.\n**Message:** {message}\n\nI will DM you with your reminder.",
                    Color = Color.Orange,
                    ThumbnailUrl = "https://i.imgur.com/SejD45x.png",
                    Timestamp = DateTimeOffset.UtcNow
                }
        .WithFooter(footer =>
        {
            footer.Text = "┤|ByteKnight Discord Bot|├";
            footer.IconUrl = "https://i.imgur.com/SejD45x.png";
        });
                await slashCommand.RespondAsync(embed: embed.Build(), ephemeral: true);

                // Optionally: You can schedule immediate delivery using a delayed task here.
                // However, rely on the background service for resilience.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
        public static async Task StartCustomRemindersAsync()
        {
            while (true)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    // Find the next reminder due
                    var nextReminder = await ByteKnightEngine._reminderCollection
                        .Find(r => r.TriggerTime > now)
                        .SortBy(r => r.TriggerTime)
                        .FirstOrDefaultAsync();

                    if (nextReminder != null)
                    {
                        var timeUntilNext = nextReminder.TriggerTime - now;

                        // Wait until the next reminder is due
                        if (timeUntilNext > TimeSpan.Zero)
                        {
                            await Task.Delay(timeUntilNext);
                        }

                        // Find all reminders that are due (including any that might have been queued during the wait)
                        var dueReminders = await ByteKnightEngine._reminderCollection
                            .Find(r => r.TriggerTime <= DateTime.UtcNow)
                            .ToListAsync();

                        foreach (var reminder in dueReminders)
                        {
                            try
                            {
                                // Retrieve user and send DM
                                var user = ByteKnightEngine._client.GetUser(reminder.UserId);
                                if (user != null)
                                {
                                    var dmChannel = await user.CreateDMChannelAsync();
                                    var reminderEmbed = new EmbedBuilder
                                    {
                                        Title = "🔔 Reminder!",
                                        Description = $"**{reminder.ReminderMessage}**",
                                        ThumbnailUrl = "https://i.imgur.com/SejD45x.png",
                                        Color = Color.Gold
                                    }
                                    .WithFooter(footer =>
                                    {
                                        footer.Text = "┤|ByteKnight Discord Bot|├";
                                        footer.IconUrl = "https://i.imgur.com/SejD45x.png";
                                    })
                                    .Build();

                                    await dmChannel.SendMessageAsync(embed: reminderEmbed);
                                }
                                // Remove the reminder after sending
                                await ByteKnightEngine._reminderCollection.DeleteOneAsync(r => r.Id == reminder.Id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing reminder for user {reminder.UserId}: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        // No reminders found; wait for a default interval
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in reminder loop: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(10)); // Short delay before retrying on error
                }
            }
        }


    }
}