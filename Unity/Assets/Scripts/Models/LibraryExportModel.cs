using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Newtonsoft.Json;

namespace EpubParser.Models
{
    internal class LibraryExport
    {
        [JsonProperty("generated_at")]
        public DateTime GeneratedAt { get; set; }

        [JsonProperty("source_folder")]
        public string SourceFolder { get; set; } = string.Empty;

        [JsonProperty("total_found")]
        public int TotalFound { get; set; }

        [JsonProperty("total_parsed")]
        public int TotalParsed { get; set; }

        [JsonProperty("books")]
        public List<EpubMetadataModel> Books { get; set; } = new();

        [JsonProperty("errors")]
        public List<ParseError> Errors { get; set; } = new();
    }

}
