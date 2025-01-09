using Discord;
using Discord.WebSocket;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Setup
    {
        private readonly ByteKnightEngine _botInstance;

        public Setup(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var guildChannel = slashCommand.Channel as SocketGuildChannel;
                var guild = guildChannel?.Guild;
                var user = slashCommand.User as SocketGuildUser;

                if (guild == null || user == null)
                {
                    await slashCommand.RespondAsync("Unable to access guild or user information.", ephemeral: true);
                    return;
                }
                if (!user.GuildPermissions.Administrator)
                {
                    await slashCommand.RespondAsync("You do not have permission to use this command.", ephemeral: true);
                    return;
                }

                var channels = guild.Channels
                    .OfType<ITextChannel>()
                    .Select(c => new SelectMenuOptionBuilder
                    {
                        Label = c.Name,
                        Value = c.Id.ToString()
                    }).ToList();

                var roles = guild.Roles
                    .Where(r => !r.IsEveryone)
                    .Select(r => new SelectMenuOptionBuilder
                    {
                        Label = r.Name,
                        Value = r.Id.ToString()
                    }).ToList();

                // Create dropdown with pagination
                List<SelectMenuBuilder> CreateDropdowns(string customId, string placeholder, IEnumerable<SelectMenuOptionBuilder> options)
                {
                    var dropdowns = new List<SelectMenuBuilder>();
                    int totalPages = (int)Math.Ceiling((double)options.Count() / 25);

                    for (int i = 0; i < totalPages; i++)
                    {
                        var dropdown = new SelectMenuBuilder()
                            .WithCustomId($"{customId}_page_{i + 1}")
                            .WithPlaceholder($"{placeholder} - Page {i + 1} of {totalPages}");

                        foreach (var option in options.Skip(i * 25).Take(25))
                        {
                            dropdown.AddOption(option.Label, option.Value);
                        }

                        dropdowns.Add(dropdown);
                    }

                    return dropdowns;
                }
                // Add more mongoDB related settings here, or create a dashboard to manage, and save them.
                // It is far easier to do it via a command than creating a dashboard.
                var initialEmbed = new EmbedBuilder
                {
                    Title = "🛠️ ByteKnight Basic Setup Wizard",
                    Description = "Welcome to the **ByteKnight Basic Setup Wizard**! 🚀\n\n" +
                                  "We'll walk you through the essential configuration for your server.\n\nPlease follow these steps:\n\n" +
                                  "🔧 **Welcome Channel**: Choose the channel where new members will be greeted.\n" +
                                  // Can be used for intro embed to display link to rules channel
                                  "📜 **Rules Channel**: Pick the channel where the server rules and information will be displayed.\n" +
                                  "🎖️ **Auto Role**: Select the role that will be automatically assigned to new members upon joining.\n\n" +
                                  "🔍 Use the dropdown menus provided in the following messages to make your selections. Each section will guide you through the process.",
                    ThumbnailUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/settings.png",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Let's get started! 🎉",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    },
                    Timestamp = DateTime.UtcNow
                };

                await slashCommand.RespondAsync(embed: initialEmbed.Build(), ephemeral: true);

                var embedsAndDropdowns = new List<(EmbedBuilder, List<SelectMenuBuilder>)>
        {
            (
                new EmbedBuilder
                {
                    Title = "🔧 Verification Channel",
                    Description = "Select the channel where Verification messages & Welcome messages will be sent.",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Use the dropdown menu below to select the welcome channel.",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    },
                    Timestamp = DateTime.UtcNow
                },
                CreateDropdowns("setup_welcome_channel", "Select Verification Channel", channels)
            ),
            (
                new EmbedBuilder
                {
                    Title = "📜 Rules Channel",
                    Description = "Select the channel where the server rules & info will be posted.",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Use the dropdown menu below to select the rules channel.",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    },
                    Timestamp = DateTime.UtcNow
                },
                CreateDropdowns("setup_rules_channel", "Select Rules Channel", channels)
            ),
            (
                new EmbedBuilder
                {
                    Title = "🎖️ Auto Role",
                    Description = "Select the role that will be automatically assigned to new members after verification.",
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Use the dropdown menu below to select the auto role.",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    },
                    Timestamp = DateTime.UtcNow
                },
                CreateDropdowns("setup_autorole", "Select Auto Role", roles)
            )
        };

                var firstEmbedAndDropdowns = embedsAndDropdowns.First();
                var componentBuilder = new ComponentBuilder();

                foreach (var dropdown in firstEmbedAndDropdowns.Item2)
                {
                    var row = new ActionRowBuilder()
                        .AddComponent(dropdown.Build());

                    componentBuilder.AddRow(row);
                }

                await slashCommand.FollowupAsync(embed: firstEmbedAndDropdowns.Item1.Build(), components: componentBuilder.Build(), ephemeral: true);

                for (int i = 1; i < embedsAndDropdowns.Count; i++)
                {
                    var embedAndDropdowns = embedsAndDropdowns[i];
                    componentBuilder = new ComponentBuilder();

                    foreach (var dropdown in embedAndDropdowns.Item2)
                    {
                        var row = new ActionRowBuilder()
                            .AddComponent(dropdown.Build());

                        componentBuilder.AddRow(row);
                    }

                    await slashCommand.FollowupAsync(embed: embedAndDropdowns.Item1.Build(), components: componentBuilder.Build(), ephemeral: true);
                }

                Console.WriteLine("Setup interaction response sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling command: {ex.Message}");
                await slashCommand.RespondAsync("An error occurred while processing your request. Please try again later.", ephemeral: true);
            }
        }
    }
}
