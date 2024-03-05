using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using System.Security.Cryptography.X509Certificates;


namespace Company.Function
{
    public static class HttpGetCertificate
    {
        [FunctionName("HttpGetCertificate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string keyVaultName = "kv-vm-test-mmg";
            string thumbprint = "cert3";
            log.LogInformation("Variáveis setadas.");

            // Create an insta nce of the CertificateClient using Azure Identity
            var credential = new DefaultAzureCredential();
            var client = new CertificateClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), credential);
            log.LogInformation("Key-vault conectado.");

            // Retrieve the certificate by thumbprint (pelo thumbprint, não funciona, só pelo nome)
            KeyVaultCertificate certificate = client.GetCertificate(thumbprint);
            log.LogInformation("Executei o get por thumbprint.");

            // Extract the certificate value (e.g., for use in your application)
            X509Certificate2 x509Certificate = new X509Certificate2(certificate.Cer);
            string certificateValue = Convert.ToBase64String(x509Certificate.RawData);

            log.LogInformation($"Certificate subject: {x509Certificate.Subject}");
            log.LogInformation($"Thumbprint: {x509Certificate.Thumbprint}");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
