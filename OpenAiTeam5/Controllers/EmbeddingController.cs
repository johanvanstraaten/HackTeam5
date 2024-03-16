using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using OpenAiTeam5.Models;
using System.Text;

namespace OpenAiTeam5.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Embedding")]
    public class EmbeddingController : Controller
    {
        [HttpPost]
        public IActionResult Index(EmbeddingRequest request)
        {
            var inputs = (request?.Inputs ?? new string[0])?.ToList();

            string endpoint = Environment.GetEnvironmentVariable("oai_endpoint", EnvironmentVariableTarget.Machine);
            string oaiKey = Environment.GetEnvironmentVariable("aoi_key", EnvironmentVariableTarget.Machine);

            Uri oaiEndpoint = new Uri(endpoint);

            AzureKeyCredential credentials = new AzureKeyCredential(oaiKey);

            OpenAIClient openAIClient = new OpenAIClient(oaiEndpoint, credentials);

            EmbeddingsOptions embeddingOptions = new EmbeddingsOptions("hack5embedding", inputs);

            var returnValue = openAIClient.GetEmbeddings(embeddingOptions);

            StringBuilder builder = new StringBuilder();

            foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
            {
                builder.AppendLine($"{item}");
            }

            var embeddings = returnValue.Value.Data.FirstOrDefault()?.Embedding.ToArray() ?? new float[0];

            return Ok(new EmbeddingResponse()
            {
                Outputs = embeddings
            });
        }
    }
}
