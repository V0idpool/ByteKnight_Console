using ByteKnightConsole.ByteKnightCore.CommandModules;
using ByteKnightConsole.MongoDBDriver;
using Discord;
using System.Net;
//
//                                                                                                                                  ByteKnight - Console
//                                                                                                              Version: 1.0.0 (Public Release - Console Version)
//                                                                                                             Author: ByteKnight Development Team (Voidpool)
//                                                                                                                           Release Date: [01/08/2025]
//
// Description:
// ByteKnight is a powerful, multi-purpose Discord bot built on top of Discord.Net and MongoDB, presented here in a streamlined console-based application.
// It offers a robust set of moderation tools (mute, ban, warn), a flexible leveling/XP system, numerous slash commands, automated role assignments, and more.
// Perfect for server admins who want a feature-rich, easily customizable bot—no GUI overhead required.
//
// Current Features:
//  - Auto Role assignment on join
//  - Role management (e.g., mute system) 
//  - User XP and level tracking, complete with leaderboards
//  - Customizable welcome messages and channels
//  - Full moderation toolkit: warnings, kicks, bans, purge, etc.
//  - Slash commands (roll, coinflip, 8ball, YouTube search, and more)
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
    public class InitCore
    {
        private readonly ByteKnightEngine _botInstance;

        // Constructor accepts the bot engine
        public InitCore(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task OnBotConnected()
        {
            // Initialize MongoDB
            await Init.InitializeMongoDBAsync();

            // ServerId(s) to pass to periodic checks
            var serverId = _botInstance._serverId;

            // If MongoDB is initialized successfully, proceed with other tasks
            var tasks = new List<Task>
    {
        Mute.LoadAndScheduleMutesAsync(),
        Warn.RemoveOldWarnings(),
        Warn.SetupWarningCleanup()
    };

            // Utilize this to run periodic timers in non-static classes
            //Below is for a multi-guild setup, not included but easy to make.
            //tasks.AddRange(serverIds.Select(serverId => ByteKnightEngine.PeriodicTasksHere(serverId)));
            //await Task.WhenAll(tasks);
           // To run non-blocking tasks use _ = TaskMethod(arg);
        }
        
        public async void OnBotDisconnected(string message)
        {
            // Handle stuff here when the bot disconnects if needed
            Console.WriteLine($"[SYSTEM]  ByteKnight disconnected...");


        }

      
    }
}
