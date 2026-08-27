using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public class EpubMetadataModel
    {
        public Guid bookID { get; set; }
        [JsonProperty("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [JsonProperty("file_name")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("file_size_bytes")]
        public long FileSizeBytes { get; set; }

        [JsonProperty("isbn")]
        public string ISBN { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("authors")]
        public List<string> Authors { get; set; } = new();

        [JsonProperty("publishers")]
        public List<string> Publishers { get; set; } = new();

        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("identifier")]
        public string Identifier { get; set; } = string.Empty;

        [JsonProperty("page_count")]
        public string PageCount { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("subjects")]
        public List<string> Subjects { get; set; } = new();

        [JsonProperty("rights")]
        public string Rights { get; set; } = string.Empty;

        [JsonProperty("publishedDate")]
        public string PublishedDate { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("format")]
        public string Format { get; set; } = string.Empty;

        [JsonProperty("source")]
        public string Source { get; set; } = string.Empty;

        [JsonProperty("relation")]
        public string Relation { get; set; } = string.Empty;

        [JsonProperty("coverage")]
        public string Coverage { get; set; } = string.Empty;

        [JsonProperty("series")]
        public string Series { get; set; } = string.Empty;

        [JsonProperty("series_index")]
        public string SeriesIndex { get; set; } = string.Empty;

        [JsonProperty("epub_version")]
        public string EpubVersion { get; set; } = string.Empty;

        [JsonProperty("cover_image_href")]
        public string CoverImageHref { get; set; } = string.Empty;

        [JsonProperty("averageRating")]
        public double AverageRating { get; set; } = 0.0;

        [JsonProperty("ratingsCount")]
        public int RatingsCount { get; set; } = 0;
        public string PageCountSource { get; set; } = string.Empty;
        public List<BookSeriesEntry> SeriesMemberships { get; set; } = new();

        // TRANSIENT //
        public byte[] CoverBytesPendingCache {  get; set; }
        public string CoverFileExtension {  get; set; }
    }
}
