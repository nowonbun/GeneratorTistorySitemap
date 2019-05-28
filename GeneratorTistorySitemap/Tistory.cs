using Newtonsoft.Json;

namespace GeneratorTistorySitemap
{
    class Tistory
    {
        [JsonProperty("item")]
        public Item Item { get; set; }
    }
}
