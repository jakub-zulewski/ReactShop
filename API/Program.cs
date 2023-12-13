using API.Data;
using API.Middleware;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<StoreContext>(options
    => options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

builder.Services.AddCors();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(options
    => options
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:3000"));

app.UseAuthorization();

app.MapControllers();

var scope = app.Services.CreateScope();
var storeContext = scope.ServiceProvider.GetRequiredService<StoreContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    storeContext.Database.Migrate();

    DbInitializer.Initialize(storeContext);
}
catch (Exception exception)
{
    logger.LogError(exception, "A problem occurred during migration.");
}

app.Run();
