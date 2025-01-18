using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{
    /// <summary>
    /// Helper class to deserialize Top.gg vote check responses.
    /// </summary>
    public class TopGGVoteResponse
    {
        [JsonProperty("voted")]
        public int Voted { get; set; }
    }
}
