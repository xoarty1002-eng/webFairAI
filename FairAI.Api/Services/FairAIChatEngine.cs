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

        var state = new StateModel();
        var words = normalizedPrompt
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var word in words)
        {
            var entry = _db.LanguageEntries.FirstOrDefault(x => x.Word == word);
            if (entry is null)
            {
                entry = new LanguageEntry
                {
                    Word = word,
                    DepthValue = Random.Shared.NextDouble(),
                    HistoryValue = Random.Shared.NextDouble(),
                    UsageCount = 1,
                    CreatedAt = DateTime.UtcNow
                };
                _db.LanguageEntries.Add(entry);
            }
            else
            {
                entry.UsageCount += 1;
            }

            state.DepthValue = (state.DepthValue + entry.DepthValue) / 2;
            state.HistoryValue = (state.HistoryValue + entry.HistoryValue) / 2;
        }

        var languagePool = new LanguagePool();
        foreach (var entry in _db.LanguageEntries.AsNoTracking().ToList())
        {
            languagePool.Data.Add(new DataModel
            {
                Word = entry.Word,
                DepthValue = entry.DepthValue,
                HistoryValue = entry.HistoryValue
            });
        }

        var depthPool = new DepthPool(32);
        var node = depthPool.Down(state);
        var coreDepth = new CoreDepth(8);
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

        foreach (var neuron in depthPool.Pool)
        {
            _db.NeuronRecords.Add(new NeuronRecord
            {
                Session = session,
                Value = neuron.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var core in coreDepth.Cores)
        {
            _db.CoreRecords.Add(new CoreRecord
            {
                Session = session,
                Range = (int)core.Range,
                Speed = core.Speed,
                Position = core.Position,
                CreatedAt = DateTime.UtcNow
            });
        }

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
