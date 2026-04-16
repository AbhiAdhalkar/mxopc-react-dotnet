using System.Collections.Concurrent;
using System.Text.Json;
using OpcLabs.EasyOpc.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://mxopc-react-dotnet.vercel.app"
                //"https://mxopc-react-dotnet-fkhrbr5nc-abhidhalkars-projects.vercel.app",
                //"https://mxopc-react-dotnet-28aw2zzoy-abhidhalkars-projects.vercel.app",
                //"https://mxopc-react-dotnet-nenotjiao-abhidhalkars-projects.vercel.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<OpcStore>();

var app = builder.Build();

app.UseCors("ReactPolicy");

var client = new EasyDAClient();
var store = app.Services.GetRequiredService<OpcStore>();

var json = File.ReadAllText("tags.json");

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

var config = JsonSerializer.Deserialize<OpcTagConfig>(json, jsonOptions);

if (config == null || string.IsNullOrWhiteSpace(config.ServerName) || config.Tags.Count == 0)
{
    throw new Exception("Invalid tags.json configuration.");
}

var machineName = string.IsNullOrWhiteSpace(config.MachineName) ? "" : config.MachineName;

client.ItemChanged += (_, e) =>
{
    try
    {
        var itemId = e.Arguments?.ItemDescriptor?.ItemId ?? "";
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        if (e.Succeeded)
        {
            store.Set(new TagData
            {
                TagName = itemId,
                Value = NormalizeValue(e.Vtq?.Value),
                Quality = e.Vtq?.Quality?.ToString(),
                Timestamp = e.Vtq?.Timestamp.ToString(),
                Error = null
            });
        }
        else
        {
            store.Set(new TagData
            {
                TagName = itemId,
                Value = null,
                Quality = "Error",
                Timestamp = null,
                Error = e.ErrorMessageBrief
            });
        }
    }
    catch
    {
    }
};

foreach (var tag in config.Tags)
{
    store.Set(new TagData
    {
        TagName = tag,
        Value = "0",
        Quality = "Initializing",
        Timestamp = null,
        Error = null
    });

    client.SubscribeItem(
        machineName,
        config.ServerName,
        tag,
        config.UpdateRate
    );
}

app.MapMethods("/api/tags/write", new[] { "OPTIONS" }, () => Results.Ok());

app.MapGet("/api/tags", () =>
{
    return Results.Ok(store.GetAll());
});

app.MapGet("/api/tags/{tagName}", (string tagName) =>
{
    var tag = store.Get(tagName);
    return tag is null ? Results.NotFound() : Results.Ok(tag);
});

app.MapPost("/api/tags/write", (ToggleTagRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.TagName))
        return Results.BadRequest("TagName is required.");

    try
    {
        var current = store.Get(request.TagName)?.Value;
        var nextValue = string.IsNullOrWhiteSpace(current) || current == "0" ? "1" : "0";
        var valueToWrite = nextValue == "1" ? 1 : 0;

        client.WriteItemValue(
            machineName,
            config.ServerName,
            request.TagName,
            valueToWrite
        );

        store.Set(new TagData
        {
            TagName = request.TagName,
            Value = nextValue,
            Quality = "Good",
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Error = null
        });

        return Results.Ok(new
        {
            success = true,
            tagName = request.TagName,
            value = nextValue
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = ex.ToString()
        });
    }
});

app.Run();

static string? NormalizeValue(object? value)
{
    if (value == null) return null;

    if (value is bool b) return b ? "1" : "0";

    if (value is byte by) return by != 0 ? "1" : "0";
    if (value is short s) return s != 0 ? "1" : "0";
    if (value is int i) return i != 0 ? "1" : "0";
    if (value is long l) return l != 0 ? "1" : "0";
    if (value is float f) return f != 0 ? "1" : "0";
    if (value is double d) return d != 0 ? "1" : "0";
    if (value is decimal m) return m != 0 ? "1" : "0";

    var text = value.ToString()?.Trim().ToLowerInvariant();

    if (text == "true" || text == "1" || text == "on")
        return "1";

    if (text == "false" || text == "0" || text == "off")
        return "0";

    return text;
}

public class OpcStore
{
    private readonly ConcurrentDictionary<string, TagData> _tags = new();

    public void Set(TagData tag)
    {
        _tags[tag.TagName] = tag;
    }

    public TagData? Get(string tagName)
    {
        _tags.TryGetValue(tagName, out var value);
        return value;
    }

    public List<TagData> GetAll()
    {
        return _tags.Values.OrderBy(x => x.TagName).ToList();
    }
}

public class OpcTagConfig
{
    public string MachineName { get; set; } = "";
    public string ServerName { get; set; } = "";
    public int UpdateRate { get; set; } = 500;
    public List<string> Tags { get; set; } = new();
}

public class TagData
{
    public string TagName { get; set; } = "";
    public string? Value { get; set; }
    public string? Quality { get; set; }
    public string? Timestamp { get; set; }
    public string? Error { get; set; }
}

public class ToggleTagRequest
{
    public string TagName { get; set; } = "";
}