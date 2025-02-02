using ByteKnightConsole.ByteKnightCore.CommandModules;
using Discord;
using Discord.WebSocket;
//
//                                                                                                                                  ByteKnight - CLI
//                                                                                                              Version: 1.4.0 (Public Release - CLI Version)
//                                                                                                             Author: ByteKnight Development Team (Voidpool)
//                                                                                                                           Release Date: [01/18/2025]
//
// Description:
// ByteKnight is a powerful, multi-purpose Discord bot built on top of Discord.Net and MongoDB, presented here in a streamlined console-based application.
// It offers a robust set of moderation tools (mute, ban, warn), a flexible leveling/XP system, numerous slash commands, automated role assignments, and more.
// Perfect for server admins who want a feature-rich, easily customizable bot—no GUI overhead required.
//
// Current Features:
//  - Verification & Verification & Auto Role assignment on join
//  - Role management (e.g., mute system) 
//  - User XP and level tracking, complete with leaderboards
//  - Customizable welcome messages and channels
//  - Full moderation toolkit: warnings, kicks, bans, purge, etc.
//  - Slash commands (roll, coinflip, 8ball, YouTube search, and more)
//  - Prefix commands
//
// Future Features (Subject to change as development continues):
//  - Custom embed creation for server admins
//  - Advanced moderation logging and analytics
//  - Expanded slash commands and interactive event handling
//  - Additional integrations for streaming and social platforms
//
// Notes:
//  - This public release focuses on the console-based core bot features.
//  - CodeForge offers a GUI-based version of ByteKnight with working examples for in-app backend settings (MongoDB, etc.) 
//    available under the "ByteKnight Apprentice" tier and the "ByteKnight Champion (AI)" tier.
//  - Both paid tiers provide more command modules, advanced commands in easy-to-use module formats, and dedicated support 
//    for additions/customizations.
//  - Expect ongoing updates, new commands, and refinements.
//  - Community feedback and contributions are always welcome!
//  - Support/donate at: https://buymeacoffee.com/byteknight
//  - Join the ByteKnight Discord (support, coding help, feature requests): https://discord.gg/trm9qEzcuw
//
namespace ByteKnightConsole.ByteKnightCore
{
    /// <summary>
    /// Handles the registration and management of slash commands for the ByteKnight Discord bot.
    /// This class facilitates bulk registration of commands, ensuring commands are properly
    /// defined and updated without requiring bot restarts or manual intervention.
    /// </summary>
    public class SlashCommandService
    {
        private readonly DiscordSocketClient _client;
        /// <summary>
        /// Initializes a new instance of the <see cref="SlashCommandService"/> class.
        /// </summary>
        /// <param name="client">The Discord socket client used for bot operations.</param>
        public SlashCommandService(DiscordSocketClient client)
        {
            _client = client;
        }
        /// <summary>
        /// Registers all slash commands with the Discord API in bulk.
        /// This method defines a comprehensive list of commands, builds them, and updates their
        /// registration using Discord's bulk overwrite functionality.
        /// </summary>
        /// <remarks>
        /// - Includes a variety of command categories, such as moderation, utility, user engagement,
        ///   and admin tools.
        /// - Supports options for commands, such as specifying users, amounts, durations, and custom
        ///   messages.
        /// - Uses `BulkOverwriteGlobalApplicationCommandsAsync` to efficiently manage command updates.
        /// </remarks>
        /// <returns>An asynchronous task representing the registration process.</returns>
        public async Task RegisterSlashCommandsAsync()
        {
            // New helper method to cleanup slashcommand registrations
           //var givexpCommand = SlashCommandExtensions.CreateSlashCommand("givexp","Give XP to a user.",("user", "The user to give XP to.", ApplicationCommandOptionType.User, true),("amount", "The amount of XP to give.", ApplicationCommandOptionType.Integer, true));

            var commands = new List<SlashCommandBuilder>
    {

              new SlashCommandBuilder()
            .WithName("givexp")
            .WithDescription("Give XP to a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to give XP to.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("amount")
                .WithDescription("The amount of XP to give.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)),

                 new SlashCommandBuilder()
            .WithName("giveallxp")
            .WithDescription("Give XP to all users.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("amount")
                .WithDescription("The amount of XP to give.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)),

               new SlashCommandBuilder()
            .WithName("removexp")
            .WithDescription("Remove XP from a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to remove XP from.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("amount")
                .WithDescription("The amount of XP to remove.")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)),

               new SlashCommandBuilder()
            .WithName("coinflip")
            .WithDescription("Flip a coin, Heads or Tails."),

                new SlashCommandBuilder()
            .WithName("mute")
            .WithDescription("Mute a user for a specified duration.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to mute")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("duration")
                .WithDescription("The duration in minutes")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithRequired(true)),

                 new SlashCommandBuilder()
            .WithName("unmute")
            .WithDescription("Unmute a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to mute")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true)),


            new SlashCommandBuilder()
            .WithName("roll")
            .WithDescription("Roll the dice"),

         new SlashCommandBuilder()
            .WithName("googleit")
            .WithDescription("Send a Google link for users that won't Google it.")
            .AddOption(new SlashCommandOptionBuilder()
            .WithName("message")
            .WithDescription("The search you'd like to share to the user.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)),


         new SlashCommandBuilder()
            .WithName("say")
            .WithDescription("Make the bot say something.")
            .AddOption(new SlashCommandOptionBuilder()
            .WithName("message")
            .WithDescription("The message you'd like the bot to say.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)),

           new SlashCommandBuilder()
            .WithName("yt")
            .WithDescription("Post a YouTube link")
            .AddOption(new SlashCommandOptionBuilder()
            .WithName("query")
            .WithDescription("Search for a video, or song.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)),

           new SlashCommandBuilder()
        .WithName("live")
        .WithDescription("Alert when a user is live on Twitch")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("twitch-name")
            .WithDescription("Your Twitch Username")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("game-name")
            .WithDescription("The game being played")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)),

         new SlashCommandBuilder()
    .WithName("purge")
    .WithDescription("Delete a specified number of messages from the channel (with optional filtering).")
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("messages")
        .WithDescription("The number of messages to delete.")
        .WithType(ApplicationCommandOptionType.Integer)
        .WithRequired(true))
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("user")
        .WithDescription("Delete messages from a specific user.")
        .WithType(ApplicationCommandOptionType.User)
        .WithRequired(false))
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("keyword")
        .WithDescription("Delete messages containing a specific keyword/phrase.")
        .WithType(ApplicationCommandOptionType.String)
        .WithRequired(false))
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("bots-only")
        .WithDescription("Delete messages from bots only.")
        .WithType(ApplicationCommandOptionType.Boolean)
        .WithRequired(false)),

            new SlashCommandBuilder()
            .WithName("pm")
            .WithDescription("Send a PM to a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to PM.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                .WithName("message")
                .WithDescription("The message to send.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)),

            new SlashCommandBuilder()
            .WithName("8ball")
            .WithDescription("Ask the magic 8 ball a question.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("question")
                .WithDescription("The question you want to ask")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)),

        new SlashCommandBuilder()
            .WithName("help")
            .WithDescription("Display information about ByteKnight, and its features."),

        new SlashCommandBuilder()
            .WithName("level")
            .WithDescription("Check and display your Server Level"),

        new SlashCommandBuilder()
            .WithName("leaderboard")
            .WithDescription("Check and display the Server Leaderboard"),

        new SlashCommandBuilder()
        .WithName("rank")
        .WithDescription("Display user rank information.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("user")
            .WithDescription("The user to display rank information for.")
            .WithType(ApplicationCommandOptionType.User)
            .WithRequired(true)),

            new SlashCommandBuilder()
            .WithName("kick")
            .WithDescription("Kick a user from the server.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to kick.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true)),

        new SlashCommandBuilder()
            .WithName("softban")
            .WithDescription("Softban a user from the server.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to softban.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("reason")
                .WithDescription("The reason for the softban.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false)),

        new SlashCommandBuilder()
            .WithName("ban")
            .WithDescription("Ban a user from the server.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to ban.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("reason")
                .WithDescription("The reason for the ban.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(false)),

            new SlashCommandBuilder()
            .WithName("setup")
            .WithDescription("Start the setup wizard for ByteKnight.(Admin/Owners Only)"),

         new SlashCommandBuilder()
            .WithName("warn")
            .WithDescription("Issue a warning to a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to warn.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("reason")
                .WithDescription("The reason for the warning.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)),

         new SlashCommandBuilder()
            .WithName("warninfo")
            .WithDescription("View recent warning reason for specified user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("Check most recent warning for specified user.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true)),

         new SlashCommandBuilder()
            .WithName("warnclear")
            .WithDescription("Clear a specified users warnings.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("Clear a specified users warnings.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true))
         .AddOption(new SlashCommandOptionBuilder()
                .WithName("reason")
                .WithDescription("Select Warning ID to Remove.")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)),

        new SlashCommandBuilder()
    .WithName("duel")
    .WithDescription("Challenge a user to a duel.")
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("user")
        .WithDescription("The user to duel.")
        .WithType(ApplicationCommandOptionType.User)
        .WithRequired(true)),

           new SlashCommandBuilder()
            .WithName("steal")
            .WithDescription("Rob a random user of their XP (once per 6 hours)."),

               new SlashCommandBuilder()
    .WithName("updatemsg")
    .WithDescription("Send a server update message")
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("title")
        .WithDescription("The title of the message.")
        .WithType(ApplicationCommandOptionType.String)
        .WithRequired(true))
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("message")
        .WithDescription("The content of the message. (Discord Markdown supported use \n for new lines!")
        .WithType(ApplicationCommandOptionType.String)
        .WithRequired(true)),

                new SlashCommandBuilder()
            .WithName("slap")
            .WithDescription("Slap TF out of a user.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to slap TF out of.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true)),

                  new SlashCommandBuilder()
            .WithName("verify")
            .WithDescription("Manually verify a user, and give them the server access AutoRole.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("user")
                .WithDescription("The user to manually verify.")
                .WithType(ApplicationCommandOptionType.User)
                .WithRequired(true)),

                  new SlashCommandBuilder()
            .WithName("reminders")
            .WithDescription("Opt in to get DM Reminders for events, votes, and steal cooldowns 10 min before it happens."),

                    new SlashCommandBuilder()
    .WithName("remindme")
    .WithDescription("Set a personal reminder.")
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("time")
        .WithDescription("Time in minutes before the reminder.")
        .WithType(ApplicationCommandOptionType.Integer) // Time is passed as an integer
        .WithRequired(true))
    .AddOption(new SlashCommandOptionBuilder()
        .WithName("message")
        .WithDescription("The reminder message.")
        .WithType(ApplicationCommandOptionType.String) // Message is passed as a string
        .WithRequired(true)),

                  new SlashCommandBuilder()
            .WithName("vote")
            .WithDescription("Vote for BotPulse Daily on Top.gg. Support its growth, and get 100 XP every time you vote!"),

                  new SlashCommandBuilder()
            .WithName("theftleaderboard")
            .WithDescription("Check the Top 10 Thieves! (Resets Monthly)."),

                  new SlashCommandBuilder()
            .WithName("theftstats")
            .WithDescription("Check your Theft Stats or another users (Resets Monthly).")
             .AddOption(new SlashCommandOptionBuilder()
              .WithName("user")
            .WithDescription("The users theft stats to check (Leave blank to check your own)")
            .WithType(ApplicationCommandOptionType.User)
            .WithRequired(false)),

            // Add more commands here

        };
            // Build the commands from the builders
            var commandsbuild = commands.Select(builder => builder.Build()).ToArray();

            try
            {
                // Use BulkOverwriteGlobalApplicationCommandsAsync for bulk registration and updates (No longer need to kick the bot to reload commands!)
                await _client.BulkOverwriteGlobalApplicationCommandsAsync(commandsbuild);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering commands: {ex.Message}");
            }


        }
    }
}
