using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net;

namespace Company.Function
{
    public class TimerTriggerCaRootChecking
    {
        [FunctionName("TimerTriggerCaRootChecking")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string fileUrl = "https://acraiz.icpbrasil.gov.br/credenciadas/CertificadosAC-ICP-Brasil/hashsha512.txt";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Bypass SSL certificate validation (since CA is not available)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    HttpResponseMessage response = await client.GetAsync(fileUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        Stream contentStream = await response.Content.ReadAsStreamAsync();

                        // Read the file content
                        using (StreamReader reader = new StreamReader(contentStream))
                        {
                            string fileContent = await reader.ReadToEndAsync();
                            log.LogInformation($"Contents of {fileUrl}:\n{fileContent}");
                        }
                    }
                    else
                    {
                        log.LogInformation($"Error downloading file: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"An error occurred: {ex.Message}");
            }
        }
    }
}
