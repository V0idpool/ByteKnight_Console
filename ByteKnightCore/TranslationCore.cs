using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.ByteKnightCore
{
    public class TranslationCore
    {
        public static async Task<string> GoogleTranslation(string text, string targetLanguage)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLanguage}&dt=t&q={Uri.EscapeDataString(text)}";
                    var response = await client.GetStringAsync(url);
                    var json = JArray.Parse(response);
                    return json[0][0][0]?.ToString() ?? "Translation error.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Translation Exception: {ex.Message}");
                    return "⚠️ Translation failed.";
                }
            }
        }
        public static async Task<string> TranslateTextAsync(string text, string targetLanguage)
        {
            string translatedText = await GoogleTranslation(text, targetLanguage);
            return string.IsNullOrWhiteSpace(translatedText) ? "⚠️ Translation failed." : translatedText;
        }
        // Define the language name to code maps. (This stuff helps with sending the correct language code to Google Translate) You can probably find more.
        public static readonly Dictionary<string, string> LanguageCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "english", "en" }, { "spanish", "es" }, { "french", "fr" }, { "german", "de" }, { "italian", "it" }, { "russian", "ru" },
            { "chinese", "zh" }, { "japanese", "ja" }, { "korean", "ko" }, { "portuguese", "pt" }, { "dutch", "nl" }, { "arabic", "ar" },
            { "hindi", "hi" }, { "turkish", "tr" }, { "polish", "pl" }, { "swedish", "sv" }, { "danish", "da" }, { "greek", "el" },
            { "hebrew", "iw" }, { "thai", "th" }, { "vietnamese", "vi" }, { "indonesian", "id" }
        };


    }
}
