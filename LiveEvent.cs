using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trinket
{
    class LiveEvent : EventArgs
    {
        [JsonPropertyName("eventname")]
        public string EventName { get; set; }

        [JsonPropertyName("other")]
        public string Other { get; set; }

        [JsonPropertyName("otherID")]
        public string OtherID { get; set; }

        [JsonPropertyName("otherTeam")]
        public string OtherTeam { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("sourceID")]
        public string SourceID { get; set; }

        [JsonPropertyName("sourceTeam")]
        public string SourceTeam { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
