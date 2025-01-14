using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Help
    {
        private readonly ByteKnightEngine _botInstance;

        public Help(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var embed = new EmbedBuilder
                {
                    Title = "🤖 **ByteKnight - Open Source C# Discord Bot** 🤖",
                    Description = "Welcome to **Code Forge**! A community for coders of all skill levels and languages to share, collaborate, and learn together. Here's everything you need to know:\n\n" +
                           "💻 **About ByteKnight:**\n" +
                           "ByteKnight is an open-source C# Discord bot packed with features to enhance your server experience. By subscribing to one of our tiers, you gain access to:\n\n" +
                            "🔹 The full source code of ByteKnight for self-hosting\n" +
                            "🔹 Continuous updates and new features\n" +
                           "💎 **Subscription Tiers:**\n" +
                           "**Tier 1 - ByteKnight Apprentice**\n" +
                           "🔹 [**Monthly Plan**](https://buy.stripe.com/3cs00Kb1w2ys4F26ot)\n" +
                           "🔹 [**Yearly Plan**](https://buy.stripe.com/fZe6p8b1w6OIb3q006)\n" +
                           "   - Access to basic commands\n" +
                           "   - Non-canvas commands\n" +
                           "   - Access to the **Forge Supporters Channel** for early updates\n\n" +
                           "**Tier 2 - ByteKnight Champion**\n" +
                           "🔹 [**Monthly Plan**](https://buy.stripe.com/5kA00K3z41uo1sQeV1)\n" +
                           "🔹 [**Yearly Plan**](https://buy.stripe.com/eVa28S3z46OI3AY4go)\n" +
                           "   - Access to all Tier 1 features\n" +
                           "   - **AI Commands** & Advanced commands\n" +
                           "   - Canvas-level commands and leaderboard features\n" +
                           "   - Access to the **Forge Supporters Channel** for early updates\n\n" +
                           "💬 **About Code Forge:**\n" +
                           "Join our vibrant community where coders from all backgrounds come together to:\n" +
                           "🔹 Download and get support for ByteKnight\n" +
                           "🔹 Share code and projects\n" +
                           "🔹 Get help with coding challenges\n" +
                           "🔹 Offer guidance and mentorship\n" +
                           "🔹 Discuss both coding and non-coding topics\n\n" +
                           "📜 **Join Us:**\n" +
                           "ByteKnight Subscriptions are NOT required, It's there as an option for users to get their own bot up and running.\n" +
                           "Whether you're a beginner or an expert, there's a place for you at Code Forge.\n" +
                           "🌟 [**Join the Code Forge Community Server**](https://discord.gg/trm9qEzcuw)\n\n" +
                           "Feel free to reach out if you have any questions or need assistance! 📨",
                    Color = Color.DarkRed,
                    ThumbnailUrl = "https://i.imgur.com/SejD45x.png",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "\u200B\n┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    }
                };
                await slashCommand.RespondAsync(embed: embed.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling setup command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }

    }
}
