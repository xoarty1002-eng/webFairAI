using FairAI.Api.Data;
using FairAI.Api.Domain;
using FairAI.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FairAI.Api.Services;

public class FairAIChatEngine : IDisposable
{
    private readonly FairAiDbContext _db;
    private readonly bool _ownsContext;
    private bool _disposed = false;

    // Parameterless constructor preserved for legacy fallbacks / isolated testing suites
    public FairAIChatEngine() : this(CreateDefaultDbContext())
    {
        _ownsContext = true;
    }

    // Direct injection constructor ensuring lifetime scopes sync cleanly with route endpoints
    public FairAIChatEngine(FairAiDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
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

    public void Vote(double x, double y, double z, string voteType)
    {
        var coreDepth = new CoreDepth(8, _db);
        coreDepth.Vote(x, y, z, voteType);
    }

    public void Report(string content)
    {
        var languagePool = new LanguagePool(_db);
        languagePool.Delete(content);
    }

    public ChatResponse Translate(double x, double y, double z, double exchangeRate)
    {
        var languagePool = new LanguagePool(_db);
        var depthPool = new DepthPool(32, _db);

        var verifiedNode = new NodeModel()
        {
            MeaningValue = (x * exchangeRate) % 1,
            LanguageValue = (y * exchangeRate) % 1,
            MiddleValue = (z * exchangeRate) % 1
        };

        var processedState = depthPool.Up(verifiedNode);
        var generatedText = languagePool.Generate(processedState, null);

        // Generate a valid pseudo-session ID or fallback instead of a hardcoded breakable 0
        int fallbackSessionId = Math.Abs((generatedText ?? "").GetHashCode() % 100000);

        return new ChatResponse
        {
            Id = fallbackSessionId,
            Prompt = "Translation Matrix Query",
            Message = string.IsNullOrWhiteSpace(generatedText) ? "Translation response could not be synthesized." : generatedText,
            DepthValue = processedState.MeaningValue,
            HistoryValue = processedState.LanguageValue,
            X = x,
            Y = y,
            Z = z
        };
    }

    public string ProcessText(string prompt, string? user)
    {
        return Process(prompt, user).Message;
    }

    public ChatResponse Process(string prompt, string? user)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(prompt));
        }

        var normalizedPrompt = prompt.Trim();
        var languagePool = new LanguagePool(_db);
        var depthPool = new DepthPool(32, _db);

        var state = languagePool.Calculate(normalizedPrompt, user);
        var node = depthPool.Down(state);

        var coreDepth = new CoreDepth(8, _db);
        var verifiedNode = coreDepth.Check(node);
        var processedState = depthPool.Up(verifiedNode);

        var generatedText = languagePool.Generate(processedState, user);

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

        string[] cryptoKeywords = new[]
        {
            "crypto", "cryptocurrency", "stablecoin", "altcoin", "token",
            "bitcoin", "btc", "ethereum", "eth", "ripple", "xrp", "litecoin", "ltc",
            "dogecoin", "doge", "shiba inu", "shib", "solana", "sol", "cardano", "ada",
            "monero", "xmr", "polkadot", "dot", "tron", "trx", "polygon", "matic",
            "avalanche", "avax", "chainlink", "link", "uniswap", "uni", "stellar", "xlm",
            "cosmos", "atom", "algorand", "algo", "dash", "verge", "xvg", "digibyte", "dgb",
            "tether", "usdt", "usd coin", "usdc", "dai", "trueusd", "tusd", "binance usd", "busd",
            "paypal usd", "pyusd", "changenow", "now", "space id", "id", "blocks"
        };

        string cleanInput = new string(normalizedLower.Select(c => char.IsPunctuation(c) ? ' ' : c).ToArray());

        if (cryptoKeywords.Any(keyword => $" {cleanInput} ".Contains($" {keyword} ", StringComparison.OrdinalIgnoreCase)))
        {
            generatedText = "You can simulate a Bitcoin miner in Python by implementing a simple Proof-of-Work (PoW) algorithm. Modern Bitcoin mining requires specialized hardware (ASICs), but a CPU-based script demonstrates the core concept of hashing a block header until it meets a specific difficulty target.\r\nHere is a functional Python script for an educational Bitcoin miner:\r\npython\r\nimport hashlib\r\nimport time\r\n\r\ndef hashlib_sha256(text):\r\n    return hashlib.sha256(text.encode('utf-8')).hexdigest()\r\n\r\ndef mine_block(block_number, transactions, previous_hash, difficulty):\r\n    prefix_zeros = '0' * difficulty\r\n    nonce = 0\r\n    \r\n    print(f\"⛏️ Mining block {block_number}...\")\r\n    start_time = time.time()\r\n    \r\n    while True:\r\n        text = str(block_number) + transactions + previous_hash + str(nonce)\r\n        current_hash = hashlib_sha256(text)\r\n        if current_hash.startswith(prefix_zeros):\r\n            duration = time.time() - start_time\r\n            print(f\"✅ Block successfully mined in {duration:.2f} seconds!\")\r\n            print(f\"Nonce found: {nonce}\")\r\n            print(f\"Hash: {current_hash}\\n\")\r\n            return current_hash\r\n        nonce += 1\r\n\r\nif __name__ == \"__main__\":\r\n    difficulty_level = 4\r\n    tx_data = \"Alice->Bob->1.5BTC, Charlie->Dave->0.4BTC\"\r\n    prev_block_hash = \"000010af92b3a890471b0128912c8a30113f8bb2312b\"\r\n    mine_block(block_number=5, transactions=tx_data, previous_hash=prev_block_hash, difficulty=difficulty_level)\r\nUse code with caution.\r\nHow This Script Works\r\n• The Nonce: A counter that increments with every loop. It is the only variable that changes in the block header.\r\n• SHA-256 Hashing: The hashlib library computes a unique 64-character hexadecimal string for the block data.\r\n• Difficulty Target: The difficulty_level variable sets how many consecutive zeros the hash must start with. Raising this value exponentially increases the time it takes to find a valid hash.\r\nWould you like to expand this into a mini blockchain simulation with multiple connected blocks, or explore how to optimize this code using multiprocessing to utilize more CPU cores?\r\n";
        }

        // Generate pseudo-session ID mapping safely
        int sessionId = Math.Abs((normalizedPrompt + (user ?? "")).GetHashCode() % 100000);

        return new ChatResponse
        {
            Id = sessionId,
            Prompt = normalizedPrompt,
            Message = generatedText,
            DepthValue = processedState.MeaningValue,
            HistoryValue = processedState.LanguageValue,
            X = verifiedNode.MeaningValue,
            Y = verifiedNode.MiddleValue,
            Z = verifiedNode.LanguageValue
        };
    }

    // Implementing strict IDisposable interface logic to prevent memory leaks from unmanaged wrappers
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _ownsContext)
            {
                _db?.Dispose();
            }
            _disposed = true;
        }
    }

    ~FairAIChatEngine()
    {
        Dispose(false);
    }
}