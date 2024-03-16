using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using OpenAiTeam5.Models;
using System.Text;
using Milvus.Client;
using System.Collections;

namespace OpenAiTeam5.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("Embedding")]
    public class EmbeddingController : Controller
    {
        public string CollectionName = "code_thong";

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

        [HttpPost("Load")]
        public IActionResult Load(EmbeddingRequest request)
        {
            var inputs = (request?.Inputs ?? new string[0])?.ToList();

            string endpoint = Environment.GetEnvironmentVariable("oai_endpoint", EnvironmentVariableTarget.Machine);
            string oaiKey = Environment.GetEnvironmentVariable("aoi_key", EnvironmentVariableTarget.Machine);

            Uri oaiEndpoint = new Uri(endpoint);

            AzureKeyCredential credentials = new AzureKeyCredential(oaiKey);

            OpenAIClient openAIClient = new OpenAIClient(oaiEndpoint, credentials);

            EmbeddingsOptions embeddingOptions = new EmbeddingsOptions("hack5embedding", inputs);

            var returnValue = openAIClient.GetEmbeddings(embeddingOptions);

            using MilvusClient milvusClient = new MilvusClient("192.168.11.66", "admin", "ab69420", 19530);
            
            if (!milvusClient.HasCollectionAsync(CollectionName).GetAwaiter().GetResult())
            {
                Console.WriteLine($"Collection {CollectionName} not exist");
            }

            List<ReadOnlyMemory<float>> list = new List<ReadOnlyMemory<float>>();

            foreach (EmbeddingItem embeddingItemData in returnValue.Value.Data)
            {
                list.Add(new ReadOnlyMemory<float>(embeddingItemData.Embedding.ToArray()));
            }
            
            MilvusCollection collection = milvusClient.GetCollection(CollectionName);

            MutationResult result = collection.InsertAsync(
                new FieldData[]
                {
                    FieldData.CreateFloatVector("Description", list),
                }).GetAwaiter().GetResult();

            collection.ReleaseAsync().GetAwaiter().GetResult();

            return Ok(result);
        }


        [HttpPost("Search")]
        public IActionResult Search(SearchRequest request)
        {
            string endpoint = Environment.GetEnvironmentVariable("oai_endpoint", EnvironmentVariableTarget.Machine);
            string oaiKey = Environment.GetEnvironmentVariable("aoi_key", EnvironmentVariableTarget.Machine);

            Uri oaiEndpoint = new Uri(endpoint);

            AzureKeyCredential credentials = new AzureKeyCredential(oaiKey);

            OpenAIClient openAIClient = new OpenAIClient(oaiEndpoint, credentials);

            EmbeddingsOptions embeddingOptions = new EmbeddingsOptions("hack5embedding", new[] { request.Search });

            var returnValue = openAIClient.GetEmbeddings(embeddingOptions);
            
            using MilvusClient milvusClient = new MilvusClient("192.168.11.66", "admin", "ab69420", 19530);

            if (!milvusClient.HasCollectionAsync(CollectionName).GetAwaiter().GetResult())
            {
                Console.WriteLine($"Collection \"code_thong\" not exist");
            }

            MilvusCollection collection = milvusClient.GetCollection(CollectionName);

            collection.LoadAsync().GetAwaiter().GetResult();

            //Waiting for collection loaded
            collection.WaitForCollectionLoadAsync(timeout: TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();

            List<ReadOnlyMemory<float>> list = new List<ReadOnlyMemory<float>>();

            list.Add(new ReadOnlyMemory<float>(returnValue.Value.Data[0].Embedding.ToArray()));

            SearchResults searchResult = collection.SearchAsync(
                "Description",
                list,
                SimilarityMetricType.L2,
                limit: 2)
                .GetAwaiter().GetResult();

            collection.ReleaseAsync().GetAwaiter().GetResult();

            return Ok(searchResult);
        }
    }
}
