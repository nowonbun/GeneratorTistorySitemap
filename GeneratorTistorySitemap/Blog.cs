using System;
using Newtonsoft.Json;

namespace GeneratorTistorySitemap
{
    class Blog
    {
        [JsonProperty("name")]
        public String Name { get; set; }
        [JsonProperty("blogId")]
        public String BlogId { get; set; }
        [JsonProperty("nickname")]
        public String NickName { get; set; }
    }
}
