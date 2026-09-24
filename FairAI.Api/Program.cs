using FairAI.Api.Data;
using FairAI.Api.Models;
using FairAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SkiaSharp; // Free replacement engine
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
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
            message = lib.ProcessMessageAndCombineImages(response.Message),
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
public static class lib
{
    public static string ProcessMessageAndCombineImages(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;

        // 1. Isolate the base64 tokens inside the string
        var regex = new Regex(@"data:image\/[a-zA-Z]*;base64,([^\s""]+)", RegexOptions.Compiled);
        var matches = regex.Matches(message);

        var loadedImages = new List<SKBitmap>();
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (Match match in matches)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(match.Groups[1].Value);

                // Decodes the image natively using Skia
                SKBitmap bitmap = SKBitmap.Decode(imageBytes);
                if (bitmap == null) continue;

                loadedImages.Add(bitmap);

                if (bitmap.Width > maxWidth) maxWidth = bitmap.Width;
                if (bitmap.Height > maxHeight) maxHeight = bitmap.Height;
            }
            catch
            {
                continue; // Skip corrupted image blocks safely
            }
        }

        // 2. Extract regular non-image words
        string[] chunks = message.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var textWords = new List<string>();

        foreach (var chunk in chunks)
        {
            if (!chunk.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                textWords.Add(chunk);
            }
        }
        string cleanMessage = string.Join(" ", textWords);

        // If there are no images, exit early with just the text
        if (loadedImages.Count == 0)
        {
            return cleanMessage;
        }

        // 3. Construct the combined canvas layer using fixed coordinates
        using (var outputBitmap = new SKBitmap(maxWidth, maxHeight, SKColorType.Rgba8888, SKAlphaType.Premul))
        {
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    int totalR = 0, totalG = 0, totalB = 0, totalA = 0;

                    // Loop through every extracted picture layer
                    foreach (var img in loadedImages)
                    {
                        if (x < img.Width && y < img.Height)
                        {
                            // Extract color channel data efficiently
                            SKColor color = img.GetPixel(x, y);
                            totalR += color.Red;
                            totalG += color.Green;
                            totalB += color.Blue;
                            totalA += color.Alpha;
                        }
                    }

                    // Create the final custom pixel with modulo 255 calculations applied
                    var finalColor = new SKColor(
                        (byte)(totalR % 255),
                        (byte)(totalG % 255),
                        (byte)(totalB % 255),
                        (byte)(totalA == 0 ? 255 : totalA % 255)
                    );

                    outputBitmap.SetPixel(x, y, finalColor);
                }
            }

            // Clean up loaded unmanaged bitmaps out of memory immediately
            foreach (var img in loadedImages)
            {
                img.Dispose();
            }

            // 4. Encode the combined bitmap back into PNG base64 stream string
            using (var image = SKImage.FromBitmap(outputBitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var ms = new MemoryStream())
            {
                data.SaveTo(ms);
                byte[] outputBytes = ms.ToArray();
                string imageResult = $"data:image/png;base64,{Convert.ToBase64String(outputBytes)}";

                return cleanMessage +" "+imageResult;
            }
        }
    }
}