using Discord;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore.PrefixModules
{
    public class PingPrefix
    {
        // This is an example of the converted SlashCommand /ping. To convert SlashCommands to Prefix Command Format, simply replace references to slashCommand.ReplyAsync/FollupAsync etc. to message.Channel.SendMessageAsync
        public static async Task HandlePrefixCmd(IUserMessage message)
        {
            var stopwatch = Stopwatch.StartNew(); // Start measuring time
            await message.Channel.SendMessageAsync("Calculating ping...");
            stopwatch.Stop(); // Stop measuring time

            var ping = stopwatch.ElapsedMilliseconds;
            Color embedColor;

            // Determine embed color based on ping value
            if (ping < 200)
            {
                embedColor = Color.Green; // Good ping
            }
            else if (ping < 300)
            {
                embedColor = Color.Gold; // Average ping
            }
            else
            {
                embedColor = Color.Red; // Bad ping
            }

            // Create an embed to display the ping
            var embed = new EmbedBuilder()
                .WithTitle("🏓 Pong!")
                .WithDescription($"Server ping is `{ping} ms`")
                .WithColor(embedColor)
                .WithThumbnailUrl("https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/ping.png")  // Optional: Add a relevant thumbnail
                .WithFooter(footer => footer
                    .WithText("Server Ping")
                    .WithIconUrl("https://voidbot.lol/img/vblogov2.png"))  // Optional: Add footer icon
                .Build();

            // Send the embed to the channel
            await message.Channel.SendMessageAsync(embed: embed);
        }
    }
}
