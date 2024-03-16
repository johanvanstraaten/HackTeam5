namespace OpenAiTeam5.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class EmbeddingRequest
    {
        public string[] Inputs { get; set; }
    }

    public class EmbeddingResponse
    {
        public float[] Outputs { get; set; }
    }
}
