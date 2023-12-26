using System.Text;

using API.Data;
using API.Entities;
using API.Middleware;
using API.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<StoreContext>(options
    => options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

builder.Services.AddIdentityCore<User>(options => options.User.RequireUniqueEmail = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:TokenKey"]))
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<TokenService>();

builder.Services.AddSwaggerGen(configuration =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Place your JWT token here with prefix \"Bearer \".",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    configuration.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    configuration.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            jwtSecurityScheme, Array.Empty<string>()
        }
    });

    configuration.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "React Shop API",
        Description = "API for React Shop"
    });
});

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(configuration
        => configuration.RouteTemplate = "docs/swagger/{documentname}/swagger.json");

    app.UseSwaggerUI(configuration =>
    {
        configuration.SwaggerEndpoint("/docs/swagger/v1/swagger.json", "React Shop API");

        configuration.RoutePrefix = "docs";

        configuration.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
    });
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(options
    => options
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:3000"));

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

var scope = app.Services.CreateScope();
var storeContext = scope.ServiceProvider.GetRequiredService<StoreContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    await storeContext.Database.MigrateAsync();

    await DbInitializer.Initialize(storeContext, userManager, roleManager);
}
catch (Exception exception)
{
    logger.LogError(exception, "A problem occurred during migration.");
}

app.Run();
