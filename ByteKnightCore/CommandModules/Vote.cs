using ByteKnightConsole;
using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Vote
    {
        private readonly ByteKnightEngine _botInstance;

        public Vote(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                await slashCommand.DeferAsync(ephemeral: true);

                var user = slashCommand.User;
                var userId = user.Id;
                var serverId = ((SocketGuildChannel)slashCommand.Channel).Guild.Id;
                // Your Top.gg bot ID and API token
                var topGGToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEzMTUwNDYzODAzOTMzMzY5MjQiLCJib3QiOnRydWUsImlhdCI6MTczNDk4MDI5Mn0.G1lLeY_dpE1ai53nOoGHxWArygiOT2-eDoP2KK-4Eus"; // Replace with your actual token
                var botId = "1315046380393336924"; // Replace with your bot's ID
                                                   // Check user’s vote status
                bool hasVoted = await Helpers.TOPGGVotes.CheckIfUserVotedOnTopGG(botId, topGGToken, userId);

                // Load user level and reward status
                var userLevel = await MongoDBDriver.Helpers.LoadUserLevel(userId, serverId);
                if (hasVoted && userLevel != null && (DateTime.UtcNow - userLevel.LastVoteRewardTime.GetValueOrDefault()).TotalHours < 12)
                {
                    var timeSinceLastVote = DateTime.UtcNow - userLevel.LastVoteRewardTime.GetValueOrDefault();
                    var timeRemaining = TimeSpan.FromHours(12) - timeSinceLastVote;

                    string timeRemainingFormatted = $"{timeRemaining.Hours}h {timeRemaining.Minutes}m";

                    await slashCommand.FollowupAsync(
 $"{user.Mention} 🎉 **Thanks for your support!**\n\n" +
 "🔄 **You've already voted and claimed your reward!**\n" +
 $"⏳ You can vote again in **{timeRemainingFormatted}** to earn more **XP**! 💰\n\n" +
 "🕒 *Rewards are processed in batches every **15 minutes**. Hang tight!*",
 ephemeral: true);

                }
                else
                {

                    var embed = new EmbedBuilder()
                        .WithTitle("🗳️ Vote for BotPulse on top.gg!")
                        .WithDescription("Support our Monitoring Bot **BotPulse** by voting every ***12 hours*** to earn **100 XP!**")
                        .AddField("💡 How to Vote", "Click the link below to vote for BotPulse on top.gg and claim your reward.")
                        .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/refs/heads/main/Img/voting-box.png")
                        .WithColor(Color.Gold)
                        .WithFooter("Thank you for supporting BotPulse! Your vote helps us grow and improve.", "https://i.imgur.com/NF1tYad.png")
                        .WithTimestamp(DateTime.UtcNow)
                        .Build();
                    var button = new ButtonBuilder
                    {
                        Label = $"Click here to Vote",
                        Style = ButtonStyle.Link,
                        Url = $"https://top.gg/bot/{botId}/vote"
                    };
                    await slashCommand.FollowupAsync(embed: embed, ephemeral: true, components: new ComponentBuilder().WithButton(button).Build());
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
