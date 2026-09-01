namespace FairAI.Api.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public int SessionId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double DepthValue { get; set; }
    public double HistoryValue { get; set; }
}

public class ChatHistoryItem
{
    public int SessionId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
