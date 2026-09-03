using FairAI.Api.Services;

namespace FairAI.Tests;

public class ChatEngineTests
{
    [Fact]
    public void Process_ShouldReturnResponse_ForUserPrompt()
    {
        var engine = new FairAIChatEngine();

        var result = engine.Process("Hello");

        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Contains("Hello", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
