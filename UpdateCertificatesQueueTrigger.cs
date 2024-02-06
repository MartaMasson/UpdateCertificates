using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Security.KeyVault.Certificates;
using Azure.Identity;
using System.IO;
using System.Threading.Tasks;

namespace Company.Function
{
    public class UpdateCertificatesQueueTrigger
    {
        [FunctionName("UpdateCertificatesQueueTrigger")]
        public async Task RunAsync([QueueTrigger("certupdate", Connection = "mmgcerts_STORAGE")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            string containerName = "selfsignedfiles"; // Replace with your container name
            string blobName = "mypfx.pfx"; // Replace with your blob name
            string pfxPassword = "@Teste123456"; // Replace with your PFX password


            var blobServiceClient = new BlobServiceClient(new Uri($"https://mmgcerts.blob.core.windows.net"), new DefaultAzureCredential());
            log.LogInformation($"C# Queue trigger function - Authenticating on Blob Storage using managed identity...");

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            log.LogInformation($"C# Queue trigger function - Idenitified the pfx from the blob..." + blobClient.Name);

            byte[] pfxBytes = blobClient.DownloadContent().Value.Content.ToArray();

            //var pfxContent = blobClient.OpenRead();
            //log.LogInformation($"C# Queue trigger function - Got the file...");
            //log.LogInformation($"File Content... Primeiro byte {pfxContent.ReadByte()} ");

            // Convert the pfxContent stream to a byte array
            /*byte[] pfxBytes;
            using (var memoryStream = new MemoryStream())
            {
                pfxContent.CopyTo(memoryStream);
                log.LogInformation($"C# memoryStream: {memoryStream}");
                pfxBytes = memoryStream.ToArray();
            }*/
            log.LogInformation($"C# Queue trigger function - File went to bytes...\n");
            log.LogInformation($"The file in bytes: {pfxBytes}");

            // Create a CertificateClient to access the Key Vault
            var keyVaultUri = new Uri($"https://kv-vm-test-mmg.vault.azure.net/");
            var credential = new DefaultAzureCredential();
            var certificateClient = new CertificateClient(keyVaultUri, credential);

            log.LogInformation($"C# Queue trigger function - Connected into key vault...");

            // Import the PFX file into the Key Vault as a certificate
            string certificateName = "mypfx"; // Replace with your certificate name

            var importOptions = new ImportCertificateOptions(certificateName, pfxBytes)
            {
                Password = pfxPassword
            };
            await certificateClient.ImportCertificateAsync(importOptions);

            log.LogInformation($"C# Queue trigger function - Imported into key vault...");

        }
    }
}

