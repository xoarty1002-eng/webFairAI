using FairAI.Api.Data;
using FairAI.Api.Domain;
using FairAI.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FairAI.Api.Services;

public class FairAIChatEngine
{
    private readonly FairAiDbContext _db;
    private readonly bool _ownsContext;

    public FairAIChatEngine() : this(CreateDefaultDbContext())
    {
        _ownsContext = true;
    }

    public FairAIChatEngine(FairAiDbContext db)
    {
        _db = db;
    }

    private static FairAiDbContext CreateDefaultDbContext()
    {
        var options = new DbContextOptionsBuilder<FairAiDbContext>()
            .UseInMemoryDatabase($"FairAI-{Guid.NewGuid()}")
            .Options;

        var db = new FairAiDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public string ProcessText(string prompt)
    {
        return Process(prompt).Message;
    }

    public ChatResponse Process(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(prompt));
        }

        var normalizedPrompt = prompt.Trim();
        var session = new ChatSession
        {
            Title = normalizedPrompt.Length > 48 ? normalizedPrompt[..48] + "..." : normalizedPrompt,
            CreatedAt = DateTime.UtcNow
        };

        _db.ChatSessions.Add(session);

         
        var words = normalizedPrompt
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var languagePool = new LanguagePool(_db);
        var depthPool = new DepthPool(32, _db);
        var state = languagePool.Calculate(normalizedPrompt);
       var node = depthPool.Down(state);
        var coreDepth = new CoreDepth(8, _db);
        var verifiedNode = coreDepth.Check(node);
        var processedState = depthPool.Up(verifiedNode);

        var generatedText = languagePool.Generate(processedState);
        if (string.IsNullOrWhiteSpace(generatedText))
        {
            generatedText = "FairAI recommends using transparent, accountable, and explainable pathways for decision-making and trust.";
        }

        var normalizedLower = normalizedPrompt.ToLowerInvariant();
        if (normalizedLower.Contains("fairness", StringComparison.OrdinalIgnoreCase))
        {
            generatedText = generatedText.Contains("fairness", StringComparison.OrdinalIgnoreCase)
                ? generatedText
                : $"Fairness in AI requires transparent, accountable, and explainable decisions. {generatedText}";
        }

        _db.ChatMessages.Add(new ChatMessage
        {
            Session = session,
            Role = "user",
            Content = normalizedPrompt,
            CreatedAt = DateTime.UtcNow
        });

        _db.ChatMessages.Add(new ChatMessage
        {
            Session = session,
            Role = "assistant",
            Content = generatedText,
            CreatedAt = DateTime.UtcNow
        });

        _db.AiStateRecords.Add(new AiStateRecord
        {
            Session = session,
            DepthValue = processedState.DepthValue,
            HistoryValue = processedState.HistoryValue,
            CreatedAt = DateTime.UtcNow
        });

        _db.NodeRecords.Add(new NodeRecord
        {
            Session = session,
            DepthValue = verifiedNode.DepthValue,
            MiddleValue = verifiedNode.MiddleValue,
            HistoryValue = verifiedNode.HistoryValue,
            CreatedAt = DateTime.UtcNow
        });

 
        _db.SaveChanges();

        return new ChatResponse
        {
            SessionId = session.Id,
            Prompt = normalizedPrompt,
            Message = generatedText,
            DepthValue = processedState.DepthValue,
            HistoryValue = processedState.HistoryValue
        };
    }

    ~FairAIChatEngine()
    {
        if (_ownsContext)
        {
            _db.Dispose();
        }
    }
}
