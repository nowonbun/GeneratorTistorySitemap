using System;
using Newtonsoft.Json;

namespace GeneratorTistorySitemap
{
    class Post
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public String title { get; set; }
        [JsonProperty("visibility")]
        public int Visibility { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("postUrl")]
        public String Url { get; set; }
    }
}
