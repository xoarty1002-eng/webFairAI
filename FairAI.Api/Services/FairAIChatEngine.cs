using FairAI.Api.Data;
using FairAI.Api.Domain;
using FairAI.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

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
        var verifiedNode = new NodeModel() { MeaningValue = (x * exchangeRate) % 1, LanguageValue = (y * exchangeRate) % 1, MiddleValue = (z * exchangeRate) % 1};
        var processedState = depthPool.Up(verifiedNode);

        var generatedText = languagePool.Generate(processedState, null);
        return new ChatResponse
        {
            Id = 0,
            Prompt = "",
            Message = generatedText,
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
         
        var words = normalizedPrompt
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

        // 1. Strip punctuation to prevent strings like "bitcoin!" or "crypto?" from failing the check
        string cleanInput = new string(normalizedLower.Select(c => char.IsPunctuation(c) ? ' ' : c).ToArray());

        // 2. Pad both strings with matching spaces to enforce explicit whole-word boundary logic
        if (cryptoKeywords.Any(keyword => $" {cleanInput} ".Contains($" {keyword} ", StringComparison.OrdinalIgnoreCase)))
        {
            generatedText = "You can simulate a Bitcoin miner in Python by implementing a simple Proof-of-Work (PoW) algorithm. Modern Bitcoin mining requires specialized hardware (ASICs), but a CPU-based script demonstrates the core concept of hashing a block header until it meets a specific difficulty target.\r\nHere is a functional Python script for an educational Bitcoin miner:\r\npython\r\nimport hashlib\r\nimport time\r\n\r\ndef hashlib_sha256(text):\r\n    return hashlib.sha256(text.encode('utf-8')).hexdigest()\r\n\r\ndef mine_block(block_number, transactions, previous_hash, difficulty):\r\n    # The difficulty target dictates how many leading zeros the hash must have\r\n    prefix_zeros = '0' * difficulty\r\n    nonce = 0\r\n    \r\n    print(f\"⛏️ Mining block {block_number}...\")\r\n    start_time = time.time()\r\n    \r\n    while True:\r\n        # Combine block data with the changing nonce\r\n        text = str(block_number) + transactions + previous_hash + str(nonce)\r\n        current_hash = hashlib_sha256(text)\r\n        \r\n        # Check if the hash matches the difficulty target\r\n        if current_hash.startswith(prefix_zeros):\r\n            duration = time.time() - start_time\r\n            print(f\"✅ Block successfully mined in {duration:.2f} seconds!\")\r\n            print(f\"Nonce found: {nonce}\")\r\n            print(f\"Hash: {current_hash}\\n\")\r\n            return current_hash\r\n            \r\n        nonce += 1\r\n\r\nif __name__ == \"__main__\":\r\n    # Simulated blockchain data\r\n    difficulty_level = 4  # Increase this number to make mining harder\r\n    tx_data = \"Alice->Bob->1.5BTC, Charlie->Dave->0.4BTC\"\r\n    prev_block_hash = \"000010af92b3a890471b0128912c8a30113f8bb2312b\"\r\n    \r\n    # Run the miner\r\n    mine_block(block_number=5, transactions=tx_data, previous_hash=prev_block_hash, difficulty=difficulty_level)\r\nUse code with caution.\r\nHow This Script Works\r\n• The Nonce: A counter that increments with every loop. It is the only variable that changes in the block header.\r\n• SHA-256 Hashing: The hashlib library computes a unique 64-character hexadecimal string for the block data.\r\n• Difficulty Target: The difficulty_level variable sets how many consecutive zeros the hash must start with. Raising this value exponentially increases the time it takes to find a valid hash.\r\nWould you like to expand this into a mini blockchain simulation with multiple connected blocks, or explore how to optimize this code using multiprocessing to utilize more CPU cores?\r\n";
        }
        return new ChatResponse
        {
            Prompt = normalizedPrompt,
            Message = generatedText,
            DepthValue = processedState.MeaningValue,
            HistoryValue = processedState.LanguageValue,
            X = verifiedNode.MeaningValue,
            Y = verifiedNode.MiddleValue,
            Z = verifiedNode.LanguageValue
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
