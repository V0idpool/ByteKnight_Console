using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore.PrefixModules
{
    public static class ClearCMD
    {

        public static async Task HandlePrefixCmd(IUserMessage message)
        {
            var slashCommandService = new SlashCommandService(ByteKnightEngine._client);
            if (!ByteKnightEngine.HasPermission(message))
            {
                await message.Channel.SendMessageAsync("You do not have permission to use this command.");
                return;
            }

            await message.DeleteAsync();
            await message.Channel.SendMessageAsync("[Command System] Removing commands, Please Wait...");

            await ByteKnightEngine._client.Rest.DeleteAllGlobalCommandsAsync();
            await message.Channel.SendMessageAsync("[Command System] Commands removed, waiting to refresh...\nThis could take up to a minute to complete.");

            await Task.Delay(30000); // Wait for 30 seconds

            await message.Channel.SendMessageAsync("[Command System] Registering Commands...");
            await slashCommandService.RegisterSlashCommandsAsync();
            await message.Channel.SendMessageAsync("[Command System] Successfully Refreshed Commands List.");
        }

    }
}
