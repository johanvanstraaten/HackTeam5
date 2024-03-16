using Microsoft.AspNetCore.Mvc;
using OpenAiTeam5.Models;
using System.Diagnostics;
using System.Text;
using Azure;
using Azure.AI.OpenAI;

namespace OpenAiTeam5.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string endpoint = Environment.GetEnvironmentVariable("oai_endpoint", EnvironmentVariableTarget.Machine);
            string oaiKey = Environment.GetEnvironmentVariable("aoi_key", EnvironmentVariableTarget.Machine);

            Uri oaiEndpoint = new Uri(endpoint);

            AzureKeyCredential credentials = new(oaiKey);

            OpenAIClient openAIClient = new(oaiEndpoint, credentials);

            EmbeddingsOptions embeddingOptions = new()
            {
                DeploymentName = "hack5embedding",
                Input = { "Your text string goes here" },
            };

            var returnValue = openAIClient.GetEmbeddings(embeddingOptions);

            StringBuilder builder = new StringBuilder();

            foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
            {
                builder.AppendLine($"{item}");
            }

            return View(new TestModel()
            {
                Text1 = builder.ToString()
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
