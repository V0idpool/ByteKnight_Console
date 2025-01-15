using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteKnightConsole.MongoDBSchemas
{   
    // Helper class to deserialize the vote check response from top.gg
    public class TopGGVoteResponse
    {
        [JsonProperty("voted")]
        public int Voted { get; set; }
    }
}
