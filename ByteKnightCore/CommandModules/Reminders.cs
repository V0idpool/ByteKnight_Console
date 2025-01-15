using ByteKnightConsole.MongoDBSchemas;
using ByteKnightConsole;
using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Reminders
    {
        private readonly ByteKnightEngine _botInstance;

        public Reminders(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                await slashCommand.DeferAsync(ephemeral: true);
                var user = slashCommand.User;
                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                if (user == null)
                {
                    await slashCommand.RespondAsync("This command can only be used by a valid user.", ephemeral: true);
                    return;
                }

                // Fetch the user's reminder settings from the database
                var filter = Builders<UserLevelData>.Filter.And(
                    Builders<UserLevelData>.Filter.Eq(u => u.ID, user.Id),
                    Builders<UserLevelData>.Filter.Eq(u => u.ServerId, serverId)
                );
                var userLevel = await ByteKnightEngine._userLevelsCollection.Find(filter).FirstOrDefaultAsync();

                bool voteReminderEnabled = userLevel?.VoteReminder ?? false;
                bool stealReminderEnabled = userLevel?.StealReminder ?? false;

                // Create the embed
                var embed = new EmbedBuilder()
                   .WithTitle("🔔 Reminder Settings")
                        .WithDescription("Enable or disable reminders. You'll receive notifications approximately 10 minutes before your selected events begin.")
                        .WithColor(Color.DarkGreen)
                        .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/notice.png")
                        .WithFooter(footer => footer.Text = "Click the buttons below to toggle reminders.")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();

                // Create the buttons
                var voteButtonStyle = voteReminderEnabled ? ButtonStyle.Success : ButtonStyle.Secondary;
                var voteButtonLabel = voteReminderEnabled ? "Vote Reminder ✅" : "Vote Reminder ❌";
                var voteButtonId = "toggle_vote_reminder";

                var stealButtonStyle = stealReminderEnabled ? ButtonStyle.Success : ButtonStyle.Secondary;
                var stealButtonLabel = stealReminderEnabled ? "Steal Reminder ✅" : "Steal Reminder ❌";
                var stealButtonId = "toggle_steal_reminder";

                var actionRow = new ComponentBuilder()
                    .WithButton(voteButtonLabel, voteButtonId, voteButtonStyle)
                    .WithButton(stealButtonLabel, stealButtonId, stealButtonStyle);

                // Respond with the embed and buttons
                await slashCommand.FollowupAsync(embed: embed, components: actionRow.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
