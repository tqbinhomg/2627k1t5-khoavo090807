var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddRazorPages();

// Connect to Astra Cassandra immediately at startup
var bundlePath = builder.Configuration["Astra:SecureConnectBundlePath"];
var clientId = builder.Configuration["Astra:ClientId"];
var clientSecret = builder.Configuration["Astra:ClientSecret"];
var keyspace = builder.Configuration["Astra:Keyspace"];

Console.WriteLine("=== Connecting to Astra Cassandra ===");
Console.WriteLine($"Bundle: {bundlePath}");
Console.WriteLine($"Keyspace: {keyspace}");

try
{
    var cluster = Cassandra.Cluster.Builder()
        .WithCloudSecureConnectionBundle(bundlePath)
        .WithCredentials(clientId, clientSecret)
        .Build();
    
    var session = cluster.Connect(keyspace);
    Console.WriteLine("=== Connected to Astra successfully! ===");
    
    builder.Services.AddSingleton<Cassandra.ISession>(session);
}
catch (Exception ex)
{
    Console.WriteLine($"=== ERROR connecting to Astra: {ex.Message} ===");
    Console.WriteLine($"Stack: {ex.StackTrace}");
    throw;
}
builder.Services.AddScoped<bt_tuan4.Repositories.IAstraRepository, bt_tuan4.Repositories.AstraRepository>();
builder.Services.AddScoped<bt_tuan4.Services.HotelService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serve static files (wwwroot)
app.UseStaticFiles();

app.MapRazorPages();

app.UseHttpsRedirection();

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
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
