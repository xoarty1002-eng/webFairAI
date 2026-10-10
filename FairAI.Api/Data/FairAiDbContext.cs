using FairAI.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FairAI.Api.Data;

public class FairAiDbContext : DbContext
{
    public FairAiDbContext(DbContextOptions<FairAiDbContext> options) : base(options)
    {
    }

    public DbSet<TextModel> DataSet => Set<TextModel>();
    public DbSet<NeuronModel> NeuronSet => Set<NeuronModel>();
    public DbSet<CoreModel> CoreSet => Set<CoreModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TextModel>().ToTable("dataset");
        modelBuilder.Entity<NeuronModel>().ToTable("neuronset");
        modelBuilder.Entity<CoreModel>().ToTable("coreset");

        modelBuilder.Entity<TextModel>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<NeuronModel>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<CoreModel>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        base.OnModelCreating(modelBuilder);
    }
}
