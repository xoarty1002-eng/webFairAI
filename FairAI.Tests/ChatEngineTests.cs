using FairAI.Api.Domain;
using FairAI.Api.Services;

namespace FairAI.Tests;

public class ChatEngineTests
{
    [Fact]
    public void Process_ShouldReturnResponse_ForUserPrompt()
    {
        var engine = new FairAIChatEngine();

        var result = engine.Process("FairAI");

        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Contains("FairAI", result.Message, StringComparison.OrdinalIgnoreCase);
    }

   [Fact]
   public void Process_ShouldAddResponse_ForUserPrompt()
    {
        var lp = new LanguagePool();

        var result = lp.Calculate("Hello");

        Assert.False(lp.Data.Count == 0);
        Assert.False(result == null);
        Assert.True(lp.Data.FirstOrDefault(x=> x.Word == "Hello") != null);
    }
}
