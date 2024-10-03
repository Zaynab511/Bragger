namespace Braggerbk.Models
{
    public class OpenAIResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public List<OpenAIChoice> Choices { get; set; }
    }

    public class OpenAIChoice
    {
        public int Index { get; set; }
        public OpenAIMessage Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class OpenAIMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }


}
