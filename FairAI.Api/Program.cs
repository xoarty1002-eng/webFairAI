using FairAI.Api.Data;
using FairAI.Api.Models;
using FairAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));

builder.Services.AddDbContext<FairAiDbContext>((serviceProvider, options) =>
{
    // Check if we are running under the Entity Framework design-time tool CLI
    bool isDesignTime = EF.IsDesignTime;

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        try
        {
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure();
            });
            return;
        }
        catch
        {
            // Only fallback if we aren't trying to run migrations
            if (!isDesignTime)
            {
                options.UseInMemoryDatabase("FairAI-Local-Fallback");
                return;
            }
        }
    }

    // CRITICAL FIX: If we are running migrations via CLI tool, we MUST provide a MySQL context, 
    // even if the connection string isn't active right now.
    if (isDesignTime)
    {
        var dummyConnectionString = "Server=localhost;Database=FairAIDb;Uid=root;Pwd=;";
        options.UseMySql(dummyConnectionString, serverVersion);
    }
    else
    {
        options.UseInMemoryDatabase("FairAI-Local-Fallback");
    }
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

app.MapGet("/api/status", () => new { name = "FairAI API", status = "online" });
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

app.MapPost("/api/vote", async (VoteRequest request, FairAIChatEngine engine) =>
{
    try
    {
        engine.Vote(request.X, request.Y, request.Z, request.VoteType);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
app.MapPost("/api/chat", async (ChatRequest request, FairAIChatEngine engine, FairAiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }
    try
    {
        var response = engine.Process(request.Message, request.UserName);
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
            sessionTitle = session?.Title,
            x = response.X,
            y = response.Y,
            z = response.Z
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            sessionId = 0,
            prompt = "",
            message = ex.Message,
            depthValue = 0.0,
            historyValue = 0.0,
            sessionTitle = ""
        });
    }

});
app.MapFallbackToFile("index.html"); 
app.Run();

public class VoteRequest
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public string VoteType { get; set; }
}