using Microsoft.EntityFrameworkCore;

namespace FairAI.Api.Data;

public class FairAiDbContext : DbContext
{
    public FairAiDbContext(DbContextOptions<FairAiDbContext> options) : base(options)
    {
    }

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<LanguageEntry> LanguageEntries => Set<LanguageEntry>();
    public DbSet<AiStateRecord> AiStateRecords => Set<AiStateRecord>();
    public DbSet<NodeRecord> NodeRecords => Set<NodeRecord>();
    public DbSet<NeuronRecord> NeuronRecords => Set<NeuronRecord>();
    public DbSet<CoreRecord> CoreRecords => Set<CoreRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LanguageEntry>()
            .HasIndex(x => x.Word)
            .IsUnique();

        modelBuilder.Entity<ChatMessage>()
            .Property(x => x.Content)
            .HasMaxLength(4000);

        base.OnModelCreating(modelBuilder);
    }
}

public class ChatSession
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LanguageEntry
{
    public int Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public double DepthValue { get; set; }
    public double HistoryValue { get; set; }
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AiStateRecord
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public double DepthValue { get; set; }
    public double HistoryValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NodeRecord
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public double DepthValue { get; set; }
    public double MiddleValue { get; set; }
    public double HistoryValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NeuronRecord
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CoreRecord
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public int Range { get; set; }
    public double Speed { get; set; }
    public double Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
