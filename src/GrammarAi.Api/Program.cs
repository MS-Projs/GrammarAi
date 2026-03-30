using GrammarAi.Application;
using GrammarAi.Infrastructure;
using GrammarAi.Infrastructure.Persistence;
using GrammarAi.Api.Middleware;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GrammarAI API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',') ?? ["http://localhost:3000"])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GrammarAI API v1"));
}

app.UseHangfireDashboard("/hangfire");
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Register Telegram webhook
var botToken = builder.Configuration["TelegramBot:Token"];
var webhookUrl = builder.Configuration["TelegramBot:WebhookUrl"];
if (!string.IsNullOrEmpty(botToken) && !string.IsNullOrEmpty(webhookUrl))
{
    using var scope = app.Services.CreateScope();
    var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    await bot.SetWebhook(webhookUrl + "/bot/webhook");
}

await app.RunAsync();
