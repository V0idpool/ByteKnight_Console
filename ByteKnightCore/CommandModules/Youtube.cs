using Discord;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Color = Discord.Color;

namespace ByteKnightConsole.ByteKnightCore.CommandModules
{
    public class Youtube
    {
        private readonly ByteKnightEngine _botInstance;
        string startupPath = AppDomain.CurrentDomain.BaseDirectory;
        public Youtube(ByteKnightEngine botInstance)
        {
            _botInstance = botInstance;
        }

        public async Task CommandHandler(SocketSlashCommand slashCommand)
        {
            try
            {
                var user = slashCommand.User as SocketGuildUser;
                string userfile2 = @"\UserCFG.ini";
                string youtubeAPIKey = _botInstance.UserSettings(startupPath + userfile2, "YoutubeAPIKey");
                string youtubeappname = _botInstance.UserSettings(startupPath + userfile2, "YoutubeAppName");
                string query = slashCommand.Data.Options.First().Value.ToString();

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = youtubeAPIKey,
                    ApplicationName = youtubeappname
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = query;
                searchListRequest.MaxResults = 1;

                var searchListResponse = await searchListRequest.ExecuteAsync();

                var searchResult = searchListResponse.Items.FirstOrDefault();

                if (searchResult != null)
                {
                    var videoId = searchResult.Id.VideoId;
                    var videoUrl = $"https://www.youtube.com/watch?v={videoId}";
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name = "YouTube Search Result",
                            IconUrl = "https://raw.githubusercontent.com/V0idpool/VoidBot-Discord-Bot-GUI/main/Img/youtubeico.png"
                        },
                        Title = searchResult.Snippet.Title,
                        Url = videoUrl,
                        Description = $"**Description:**\n {searchResult.Snippet.Description}",
                        Color = Color.DarkRed,
                        ThumbnailUrl = searchResult.Snippet.Thumbnails.Default__.Url,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = $"{user.GlobalName ?? user.Username} posted a YouTube search",
                            IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
                        },
                        Fields = new List<EmbedFieldBuilder>
        {
            new EmbedFieldBuilder
            {
                Name = "Channel",
                Value = searchResult.Snippet.ChannelTitle

            },
            new EmbedFieldBuilder
            {
                Name = "Video Uploaded On",
                Value = searchResult.Snippet.PublishedAtDateTimeOffset?.ToString("MM-dd-yyyy HH:mm tt")

            }
        }
                    };

                    await slashCommand.RespondAsync(embed: embed.Build());

                    Console.WriteLine($"{user.Username} posted a YouTube Search: {videoUrl}");
                }

                else
                {
                    await slashCommand.RespondAsync("No search results found.");

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
