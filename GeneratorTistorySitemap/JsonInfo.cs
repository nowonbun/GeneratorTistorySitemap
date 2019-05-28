using Newtonsoft.Json;

namespace GeneratorTistorySitemap
{
    class JsonInfo
    {
        [JsonProperty("tistory")]
        public Tistory Tistory { get; set; }
    }
}
