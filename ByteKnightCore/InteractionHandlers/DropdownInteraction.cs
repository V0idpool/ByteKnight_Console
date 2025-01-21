using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.InteractionHandlers
{
    /// <summary>
    /// Handles dropdown menu interactions within the ByteKnight Discord bot.
    /// Provides functionality to process user selections and update server settings in MongoDB.
    /// </summary>
    public static class DropdownInteraction
    {
        /// <summary>
        /// Stores the ID of the most recently sent ephemeral message, allowing it to be modified
        /// in response to subsequent dropdown interactions. This keeps interactions 'per user'.
        /// </summary>
        private static ulong? _currentEphemeralMessageId;
        /// <summary>
        /// Processes a dropdown menu selection interaction and updates server settings or responds with feedback.
        /// </summary>
        /// <param name="component">The <see cref="SocketMessageComponent"/> representing the interaction.</param>
        /// <remarks>
        /// - Acknowledges the interaction to prevent timeouts.
        /// - Validates the user's selection and updates relevant settings (e.g., welcome channel, rules channel, auto role).
        /// - Sends an embed message to provide feedback on the interaction outcome.
        /// </remarks>
        /// <exception cref="Exception">
        /// Logs errors to the console and provides feedback if an issue occurs during interaction handling.
        /// </exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task HandleDropdownSelection(SocketMessageComponent component)
        {
            try
            {
                // Acknowledge the interaction to avoid timeouts
                await component.DeferAsync();

                Console.WriteLine($"Handling dropdown interaction: {component.Data.CustomId}");
                Console.WriteLine($"User selection: {component.Data.Values.FirstOrDefault()}");

                var userSelection = component.Data.Values.FirstOrDefault();
                if (string.IsNullOrEmpty(userSelection))
                {
                    await component.ModifyOriginalResponseAsync(props => props.Content = "No selection made.");
                    return;
                }

                var guildChannel = component.Channel as SocketGuildChannel;
                var guildId = guildChannel?.Guild.Id ?? 0;

                if (guildId == 0)
                {
                    await component.ModifyOriginalResponseAsync(props => props.Content = "Unable to determine the guild ID.");
                    return;
                }

                var serverSettings = await MongoDBDriver.Helpers.GetServerSettings(guildId);

                if (serverSettings == null)
                {
                    await component.ModifyOriginalResponseAsync(props => props.Content = "Server settings could not be found.");
                    return;
                }

                var responseEmbed = new EmbedBuilder
                {
                    Color = Color.DarkRed,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "┤|ByteKnight Discord Bot|├",
                        IconUrl = "https://i.imgur.com/SejD45x.png"
                    },
                    Timestamp = DateTime.UtcNow
                };

                // Handle the dropdown selection based on CustomId
                switch (component.Data.CustomId)
                {
                    case string id when id.StartsWith("setup_welcome_channel"):
                        if (ulong.TryParse(userSelection, out var WelcomeChannelId))
                        {
                            serverSettings.WelcomeChannelId = WelcomeChannelId;
                            await MongoDBDriver.Helpers.SaveServerSettings(serverSettings);


                            responseEmbed.Title = "✅ Verification Channel Set";
                            responseEmbed.Description = $"The Verification Channel has been set to <#{WelcomeChannelId}>.";
                        }
                        else
                        {
                            responseEmbed.Title = "🚫 Error";
                            responseEmbed.Description = "Invalid channel selection.";
                        }
                        break;

                    case string id when id.StartsWith("setup_rules_channel"):
                        if (ulong.TryParse(userSelection, out var rulesChannelId))
                        {
                            serverSettings.RulesChannelId = rulesChannelId;
                            await MongoDBDriver.Helpers.SaveServerSettings(serverSettings);
                            responseEmbed.Title = "✅ Rules Channel Set";
                            responseEmbed.Description = $"The rules channel has been set to <#{rulesChannelId}>.";
                        }
                        else
                        {
                            responseEmbed.Title = "🚫 Error";
                            responseEmbed.Description = "Invalid channel selection.";
                        }
                        break;

                    case string id when id.StartsWith("setup_autorole"):
                        if (ulong.TryParse(userSelection, out var autoRoleId))
                        {
                            serverSettings.AutoRoleId = autoRoleId;
                            await MongoDBDriver.Helpers.SaveServerSettings(serverSettings);
                            responseEmbed.Title = "✅ Auto Role Set";
                            responseEmbed.Description = $"The auto role has been set to <@&{autoRoleId}>.";
                        }
                        else
                        {
                            responseEmbed.Title = "🚫 Error";
                            responseEmbed.Description = "Invalid role selection.";
                        }
                        break;

                    default:
                        responseEmbed.Title = "🚫 Error";
                        responseEmbed.Description = "Unknown D: It's fine, everything is fine.";
                        break;
                }

                var channel = component.Channel as ITextChannel;

                if (channel == null)
                {
                    await channel.SendMessageAsync("Unable to determine the channel.");
                    return;
                }

                if (_currentEphemeralMessageId.HasValue)
                {
                    var message = await channel.GetMessageAsync(_currentEphemeralMessageId.Value) as IUserMessage;
                    if (message != null)
                    {
                        await message.ModifyAsync(msg => msg.Embed = responseEmbed.Build());
                    }
                    else
                    {
                        // If the message doesn't exist, send a new one
                        var newMessage = await channel.SendMessageAsync(embed: responseEmbed.Build(), allowedMentions: AllowedMentions.None);
                        _currentEphemeralMessageId = newMessage.Id;
                    }
                }
                else
                {
                    // Send a new ephemeral message
                    var newMessage = await channel.SendMessageAsync(embed: responseEmbed.Build(), allowedMentions: AllowedMentions.None);
                    _currentEphemeralMessageId = newMessage.Id;
                }
            }
            catch (Exception ex)
            {
                var channel = component.Channel as ITextChannel;

                if (channel == null)
                {
                    await channel.SendMessageAsync("Unable to determine the channel.");
                    return;
                }
                Console.WriteLine($"Error handling dropdown selection: {ex.Message}");
            }
        }
    }
}
