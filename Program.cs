using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/db/ping", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("Mssql");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Connection string 'Mssql' is not configured"
        });
    }

    try
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        return Results.Ok(new
        {
            status = "ok",
            message = "Database connection successful"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Database connection failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
})
.WithName("DbPing")
.WithOpenApi();
var notes = new List<Note>();
var nextNoteId = 1;

app.MapPost("/api/notes", (CreateNoteRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new
        {
            error = "Title is required"
        });
    }

    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new
        {
            error = "Text is required"
        });
    }

    var note = new Note
    {
        Id = nextNoteId++,
        Title = request.Title.Trim(),
        Text = request.Text.Trim(),
        CreatedAt = DateTimeOffset.UtcNow
    };

    notes.Add(note);

    return Results.Created($"/api/notes/{note.Id}", note);
})
.WithName("CreateNote")
.WithOpenApi();

app.MapGet("/api/notes", () =>
{
    return Results.Ok(notes);
})
.WithName("GetNotes")
.WithOpenApi();

app.MapGet("/api/notes/{id:int}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);

    if (note is null)
    {
        return Results.NotFound(new
        {
            error = "Note not found"
        });
    }

    return Results.Ok(note);
})
.WithName("GetNoteById")
.WithOpenApi();

app.MapDelete("/api/notes/{id:int}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);

    if (note is null)
    {
        return Results.NotFound(new
        {
            error = "Note not found"
        });
    }

    notes.Remove(note);

    return Results.NoContent();
})
.WithName("DeleteNote")
.WithOpenApi();

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "OK",
        time = DateTimeOffset.UtcNow
    });
})
.WithName("HealthCheck")
.WithOpenApi();

app.MapGet("/version", () =>
{
    return Results.Ok(new
    {
        application = "IsLabApp",
        version = "1.0.0"
    });
})
.WithName("GetVersion")
.WithOpenApi();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class Note
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

record CreateNoteRequest(string Title, string Text);
