using FairAI.Api.Data;
using FairAI.Api.Models;
using FairAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Port=3306;Database=fairai;User=fairai;Password=fairai;";

builder.Services.AddDbContext<FairAiDbContext>((serviceProvider, options) =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        try
        {
            var detectedVersion = ServerVersion.AutoDetect(connectionString);
            options.UseMySql(connectionString, detectedVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure();
            });
            return;
        }
        catch
        {
            options.UseInMemoryDatabase("FairAI-Local-Fallback");
            return;
        }
    }

    options.UseInMemoryDatabase("FairAI-Local-Fallback");
});

builder.Services.AddScoped<FairAIChatEngine>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.SetIsOriginAllowed(origin => true);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FairAiDbContext>();

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();

// Serve static files from wwwroot (frontend build output)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("api/status", () => new { name = "FairAI API", status = "online" });
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

app.MapGet("/api/chat/sessions", async (FairAiDbContext db) =>
    Results.Ok(await db.ChatSessions
        .OrderByDescending(s => s.CreatedAt)
        .Select(s => new
        {
            s.Id,
            s.Title,
            s.CreatedAt,
            MessageCount = s.Messages.Count
        })
        .ToListAsync()));

app.MapGet("/api/chat/history", async (int sessionId, FairAiDbContext db) =>
{
    var messages = await db.ChatMessages
        .Where(m => m.SessionId == sessionId)
        .OrderBy(m => m.CreatedAt)
        .Select(m => new ChatHistoryItem
        {
            SessionId = m.SessionId,
            Role = m.Role,
            Content = m.Content,
            CreatedAt = m.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(messages);
});

app.MapPost("/api/chat", async (ChatRequest request, FairAIChatEngine engine, FairAiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var response = engine.Process(request.Message);
    var session = await db.ChatSessions
        .Include(s => s.Messages)
        .FirstOrDefaultAsync(s => s.Id == response.SessionId);

    return Results.Ok(new
    {
        sessionId = response.SessionId,
        prompt = response.Prompt,
        message = response.Message,
        depthValue = response.DepthValue,
        historyValue = response.HistoryValue,
        sessionTitle = session?.Title
    });
});
app.MapFallbackToFile("index.html"); 
app.Run();
