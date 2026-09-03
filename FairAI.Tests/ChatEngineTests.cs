using FairAI.Api.Services;

namespace FairAI.Tests;

public class ChatEngineTests
{
    [Fact]
    public void Process_ShouldReturnResponse_ForUserPrompt()
    {
        var engine = new FairAIChatEngine();

        var result = engine.Process("Tell me about fairness in AI");

        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Contains("fairness", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
