using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Security.KeyVault.Certificates;
using Azure.Identity;
using System.Threading.Tasks;
using System.Text.Json;

namespace Company.Function
{
    public class UpdateCertificatesQueueTrigger
    {
        [FunctionName("UpdateCertificatesQueueTrigger")]
        public async Task RunAsync([QueueTrigger("certupdate", Connection = "mmgcerts_STORAGE")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            // Assuming the message content is a JSON string
            var jsonContent = myQueueItem.ToString();

            // Deserialize the JSON string into a custom object
            var messageObject = JsonSerializer.Deserialize<MyCustomObject>(jsonContent);

            // Extract values from the message
            string blobName = messageObject.FileName;
            string pfxPassword = messageObject.Pwd;
            string certificateName = messageObject.certificateName;
            log.LogInformation($"C# Queue trigger function - FileName {blobName} and Password {pfxPassword} and CertificateName {certificateName}...");

            //Connecting to the blob storage
            string containerName = "selfsignedfiles"; // Replace with your container name
            var blobServiceClient = new BlobServiceClient(new Uri($"https://mmgcerts.blob.core.windows.net"), new DefaultAzureCredential());
            log.LogInformation($"C# Queue trigger function - Authenticating on Blob Storage using managed identity...");

            // Retrieving the PFX file from the blob storage and transforming it into bytes
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            log.LogInformation($"C# Queue trigger function - Idenitified the pfx from the blob..." + blobClient.Name);
            byte[] pfxBytes = blobClient.DownloadContent().Value.Content.ToArray();
            log.LogInformation($"C# Queue trigger function - File went to bytes...\n");
            log.LogInformation($"The file in bytes: {pfxBytes}");

            // Create a CertificateClient to connect and access the Key Vault
            var keyVaultUri = new Uri($"https://kv-vm-test-mmg.vault.azure.net/");
            var credential = new DefaultAzureCredential();
            var certificateClient = new CertificateClient(keyVaultUri, credential);
            log.LogInformation($"C# Queue trigger function - Connected into key vault...");

            // Import the PFX file into the Key Vault as a certificate
            var importOptions = new ImportCertificateOptions(certificateName, pfxBytes)
            {
                Password = pfxPassword // Check if the password is really needed
            };
            await certificateClient.ImportCertificateAsync(importOptions);

            log.LogInformation($"C# Queue trigger function - Imported into key vault...");

            //Deleting pfx file from blobstorage
            containerClient.DeleteBlob(blobName);
            log.LogInformation($"C# Queue trigger function - Deleted pfx file from the storage...");

        }
    }

    public class MyCustomObject
    {
        public string FileName { get; set; }
        public string Pwd { get; set; }
        public string certificateName { get; set; }
    }
}

