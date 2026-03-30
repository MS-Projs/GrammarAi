using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Infrastructure.Bot;
using GrammarAi.Infrastructure.Bot.Handlers;
using GrammarAi.Infrastructure.Persistence;
using GrammarAi.Infrastructure.Services;
using GrammarAi.Infrastructure.Workers;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using Telegram.Bot;

namespace GrammarAi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAiService, OpenAiService>();
        services.AddScoped<IStorageService, S3StorageService>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // OCR Worker (used by Hangfire)
        services.AddScoped<OcrWorker>();

        // S3 / R2
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var endpoint = config["Storage:Endpoint"]!;
            var accessKey = config["Storage:AccessKey"]!;
            var secretKey = config["Storage:SecretKey"]!;

            return new AmazonS3Client(
                new BasicAWSCredentials(accessKey, secretKey),
                new AmazonS3Config
                {
                    ServiceURL = endpoint,
                    ForcePathStyle = true
                });
        });

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(config["Redis:ConnectionString"]!));

        // Hangfire
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(config.GetConnectionString("DefaultConnection"))));
        services.AddHangfireServer();

        // Telegram Bot
        services.AddHttpClient("telegram")
            .AddTypedClient<ITelegramBotClient>((http, sp) =>
            {
                var token = config["TelegramBot:Token"]!;
                return new TelegramBotClient(token, http);
            });

        // Bot handlers
        services.AddScoped<BotUpdateHandler>();
        services.AddScoped<IdleHandler>();
        services.AddScoped<AwaitingImageHandler>();
        services.AddScoped<SolvingExerciseHandler>();
        services.AddScoped<ShowingResultHandler>();

        // Background notification service
        services.AddHostedService<BotNotificationService>();

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
