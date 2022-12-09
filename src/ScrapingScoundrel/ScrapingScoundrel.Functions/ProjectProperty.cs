namespace ScrapingScoundrel.Functions
{
    using System.Text.Json.Serialization;

    internal sealed class ProjectProperty
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

    }
}
