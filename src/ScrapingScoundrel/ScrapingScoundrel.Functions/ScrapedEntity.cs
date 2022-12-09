namespace ScrapingScoundrel.Functions
{
    using Azure;
    using Azure.Data.Tables;
    using System;

    internal sealed class ScrapedEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}