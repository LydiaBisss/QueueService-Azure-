using Azure;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace QueueStorages
{
    public class QueueStorageFunction
    {

        private readonly ILogger<QueueStorageFunction> _logger; 

        public QueueStorageFunction(ILogger<QueueStorageFunction> logger)
        {
            _logger = logger; 

        }

        [Function("ProcessOrders")] // Attribute marking this method as an Azure Function named "ProcessOrders".
        public async Task<IActionResult> Run(
    // This parameter triggers the function on HTTP requests with the specified authorization level and method (POST).
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            // Log an informational message indicating that the order processing has started.
            _logger.LogInformation("Processing order");

            // Retrieve the value of the "name" query parameter from the request.
            // This allows for personalized responses based on the user's input.
            string name = req.Query["name"];

            // Read the body of the HTTP request asynchronously to obtain the order data.
            // This is done by creating a new StreamReader on the request body stream and reading it until the end.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the request body JSON into an Order object using JsonConvert.
            // This converts the JSON representation of the order into a C# Order instance.
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            // Construct a response message based on whether the "name" parameter was provided.
            // If "name" is null or empty, provide a generic success message.
            // If "name" is present, include it in a personalized success message.
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This Http trigged successfully. Pass a name in the query string or in the request body for a personalised response"
                : $"Hello, {name}. The Http trigged function executed successfully";

            // Create a QueueServiceClient instance to connect to Azure Queue Storage.
            // The connection string is used to authenticate and authorize access to the storage account.
            QueueServiceClient processOrderClient = new QueueServiceClient("DefaultEndpointsProtocol=https;AccountName=lidscloud;AccountKey=Y8bWUZ1ASde6BU8mIjWIOekCKy5bI38grZgqxF16o4P9pI2Wf4ZInlOEUy21tacq/mRetY+38Eke+AStQ0ryfA==;EndpointSuffix=core.windows.net");

            // Get a reference to the specific queue named "processorders" for processing orders.
            QueueClient processOrderQueue = processOrderClient.GetQueueClient("orderss");

            // This includes the Order ID, Order Date, Product ID, and Customer Email for tracking.
            await processOrderQueue.SendMessageAsync($"New order by Customer {{order.CustomerId}} of Product {{order.ProductId}} with a quantity of {{order.Quantity}} priced at {{order.Price}}, and was placed on {{order.Order_Date}}");

            return new OkObjectResult(responseMessage);
        }


    }

    public class Order : ITableEntity
    {
        [Key]
        public string? OrderId { get; set; }
        public string? Customer_Name { get; set; }
        public string? Customer_LastName { get; set; }
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Order_Date { get; set; }

   

        // ITableEntity implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
