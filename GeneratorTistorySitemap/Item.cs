using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GeneratorTistorySitemap
{
    class Item
    {
        [JsonProperty("id")]
        public String Id { get; set; }
        [JsonProperty("userId")]
        public String UserId { get; set; }
        [JsonProperty("blogs")]
        public List<Blog> Blogs { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }
        [JsonProperty("posts")]
        public List<Post> Posts { get; set; }

    }
}
