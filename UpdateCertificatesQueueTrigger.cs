using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class UpdateCertificatesQueueTrigger
    {
        [FunctionName("UpdateCertificatesQueueTrigger")]
        public void Run([QueueTrigger("certupdate", Connection = "mmgcerts_STORAGE")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
