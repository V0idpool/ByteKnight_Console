using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public static class SlashCommandExtensions
    {
        /// <summary>
        /// Creates a SlashCommandBuilder with the given name, description, 
        /// and an arbitrary number of options passed as tuples.
        /// </summary>
        /// <param name="name">Name of the slash command.</param>
        /// <param name="description">Description of the slash command.</param>
        /// <param name="options">A params array of (Name, Description, OptionType, Required).</param>
        /// <returns>A configured SlashCommandBuilder.</returns>
        public static SlashCommandBuilder CreateSlashCommand(string name, string description, params (string Name, string Description, ApplicationCommandOptionType Type, bool Required)[] options)
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription(description);

            foreach (var (optionName, optionDescription, optionType, required) in options)
            {
                commandBuilder.AddOption(
                    name: optionName,
                    type: optionType,
                    description: optionDescription,
                    isRequired: required
                );
            }

            return commandBuilder;
        }
    }
}
