namespace ScrapingScoundrel.Functions
{
    using Azure;
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Configuration;
    using System.Net.Http;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using System.Linq;
    using SendGrid.Helpers.Mail;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    public sealed class ScrapeFunction
    {
        private const string projectPropertyKey = "ProjectProperty";
        private readonly TableClient tableClient;
        private readonly HttpClient httpClient;
        private readonly string endpoint;
        private readonly string toEmail;
        private readonly string fromEmail;

        public ScrapeFunction(TableClient tableClient, HttpClient httpClient, IConfiguration configuration)
        {
            this.tableClient = tableClient;
            this.httpClient = httpClient;
            this.endpoint = configuration["ScrapeEndpoint"];
            this.toEmail = configuration["ToEmail"];
            this.fromEmail = configuration["FromEmail"];
        }

        [FunctionName("ScrapeFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
            [SendGrid(ApiKey = "SendGridAppKey")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger log)
        {
            const int page = 1;
            var jsonNode = await this.GetJsonNode(page, log);
            if (jsonNode == null)
            {
                throw new InvalidOperationException("JsonNode was null unexpectedly");
            }

            var properties = await this.GetProjectPropertiesFromNodes(page, jsonNode, log);
            foreach (var property in properties)
            {
                var rowKey = property.Id.ToString();
                if (await this.PropertyExists(rowKey))
                {
                    log.LogInformation("Already know {0}, skipping email", rowKey);
                    continue;
                }

                log.LogInformation("{0} is new, adding to table", rowKey);
                _ = await this.tableClient.AddEntityAsync(new ScrapedEntity
                {
                    PartitionKey = projectPropertyKey, RowKey = rowKey
                });

                var sendGridMessage = this.CreateSendGridMessage(property);
                log.LogInformation("Sending email with subject {0}", sendGridMessage.Subject);
                await messageCollector.AddAsync(sendGridMessage);
            }
        }

        private async Task<bool> PropertyExists(string rowKey)
        {
            try
            {
                _ = await this.tableClient.GetEntityAsync<ScrapedEntity>(projectPropertyKey,
                    rowKey);
                return true;
            }
            catch (RequestFailedException e)
            {
                if (e.Status != 404)
                {
                    throw;
                }

                return false;
            }
        }

        private async Task<IEnumerable<ProjectProperty>> GetProjectPropertiesFromNodes(int page, JsonNode jsonNode, ILogger log)
        {
            var lastPage = jsonNode["meta"]!["last_page"]!.GetValue<int>();
            var data = jsonNode["data"]!.AsArray();

            var projects = new List<JsonNode>();
            projects.AddRange(data.Where(node => string.Equals(node["class"]!.GetValue<string>(), projectPropertyKey)));

            if (lastPage <= page)
            {
                return projects.Select(node => (ProjectProperty)node.Deserialize(typeof(ProjectProperty)));
            }

            for (var i = page + 1; i <= lastPage; i++)
            {
                jsonNode = await this.GetJsonNode(i, log);
                data = jsonNode["data"]!.AsArray();
                projects.AddRange(data.Where(node =>
                    string.Equals(node["class"]!.GetValue<string>(), projectPropertyKey)));
            }

            return projects.Select(node => (ProjectProperty)node.Deserialize(typeof(ProjectProperty)));
        }

        private async Task<JsonNode> GetJsonNode(int page, ILogger logger)
        {
            var requestUri = $"{this.endpoint}?query=Utrecht&range=15&page={page}";
            logger.LogInformation("Calling {0}", requestUri);
            var responseMessage = await this.httpClient.GetAsync(requestUri);
            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return JsonNode.Parse(stream);
        }

        private SendGridMessage CreateSendGridMessage(ProjectProperty projectProperty)
        {
            var builder = new StringBuilder(projectProperty.Url).AppendLine().AppendLine(projectProperty.Description);
            var sendGridMessage = new SendGridMessage
            {
                From = new EmailAddress(this.fromEmail),
                Subject = $"New entry found: {projectProperty.Name}",
                HtmlContent = builder.ToString()
            };
            sendGridMessage.AddTo(this.toEmail);
            return sendGridMessage;
        }
    }
}
