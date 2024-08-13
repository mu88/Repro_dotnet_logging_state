using System.Text.Json;
using WebApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = false;
    options.TimestampFormat = "HH:mm:ss ";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = true
    };
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/hello", () =>
    {
        var pets = new List<string> { "dog", "cat" };
        var nationalities = new List<string> { "german", "swiss" };
        app.Logger.Pets(pets);
        app.Logger.NationalitiesAndPetsInDifferentOrder(nationalities, pets);
        app.Logger.PetsAndNationalitiesInSameOrder(pets, nationalities);
    })
    .WithOpenApi();

await app.RunAsync();