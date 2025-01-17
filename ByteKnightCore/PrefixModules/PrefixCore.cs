using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore.PrefixModules
{
    public class PrefixCore
    {
        private readonly DiscordSocketClient _client;
        private readonly IMongoDatabase _database;
        private readonly ByteKnightEngine _botInstance;
        public PrefixCore(DiscordSocketClient client, IMongoDatabase database, ByteKnightEngine botInstance)
        {
            // Ensure proper instances are used and shared across project
            _client = client;
            _database = database;
            _botInstance = botInstance;
        }


            public  async Task HandlePrefixInteraction(IUserMessage message)
        {
            if (message.Author.IsBot) return;

            string content = message.Content.ToLower();

            if (content.StartsWith("!reloadcmd"))
            {
                await ClearCMD.HandlePrefixCmd(message);
            }
            else if (content.StartsWith("!ping"))
            {
                await PingPrefix.HandlePrefixCmd(message);
            }

            // Add additional prefix handlers here

        }
    }
  }
