var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
//builder.AddRedisClientBuilder("cache")
    //.WithOutputCache();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseOutputCache();

//string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("restaurants", () =>
{
    return new[]
    {
        new Restaurant(1, "Spiced Villa", "North Indian", 4.5),
        new Restaurant(2, "Tasty&Bites", "Italian", 4.2),
        new Restaurant(3, "DelhiBelly", "Punjab", 4.7),
        new Restaurant(4, "Kimchi", "Chinese", 4.3),
        new Restaurant(5, "Gourmet", "Mediterranean", 4.6),
        new Restaurant(6, "The Spice Route", "Indian", 4.4),
        new Restaurant(7, "Pasta Paradise", "Italian", 4.1),
        new Restaurant(8, "Taco Town", "Mexican", 4.8),
        new Restaurant(9, "Wok & Roll", "Chinese", 4.0),
        new Restaurant(10, "M Magic", "Mediterranean", 4.9),
        new Restaurant(11, "Curry Corner", "Indian", 4.2),
        new Restaurant(12, "Pizza Palace", "Italian", 4.5),
    };
}).WithName("GetRestaurants");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record Restaurant(int id, string name, string cuisine, double rating);

