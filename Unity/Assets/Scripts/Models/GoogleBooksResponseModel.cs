using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Models
{
    public class GoogleBooksResponse
    {
        [JsonProperty("totalItems")]
        public int TotalItems { get; set; }

        [JsonProperty("items")]
        public List<GoogleBooksItem> Items { get; set; } = new();
    }

    public class GoogleBooksItem
    {
        [JsonProperty("volumeInfo")]
        public GoogleBooksVolumeInfo VolumeInfo { get; set; } = new();

    }

    public class GoogleBooksVolumeInfo
    {
        [JsonProperty("pageCount")]
        public int PageCount { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("publishedDate")]
        public string PublishedDate { get; set; }
        [JsonProperty("industryIdentifiers")]
        public List<GoogleBooksIndustryIdentifier> IndustryIdentifiers { get; set; } = new();

        [JsonProperty("averageRating")]
        public double AverageRating { get; set; }

        [JsonProperty("ratingsCount")]
        public int RatingsCount { get; set; }
    }

    public class GoogleBooksIndustryIdentifier
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        [JsonProperty("identifier")]
        public string Identifier { get; set; } = string.Empty;
    }
}

