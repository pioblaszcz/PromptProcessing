using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptProcessing.Application.Abstractions.AI;
using PromptProcessing.Application.Abstractions.Messaging;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Infrastructure.AI;
using PromptProcessing.Infrastructure.Messaging;
using PromptProcessing.Infrastructure.Persistence;
using PromptProcessing.Infrastructure.Persistence.Repositories;

namespace PromptProcessing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructurePersistence(configuration);
        services.AddRabbitMqPublisher(configuration);

        return services;
    }

    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Database connection string is missing.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPromptJobRepository, PromptJobRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static IServiceCollection AddOllama(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(OllamaOptions.SectionName);
        var options = new OllamaOptions
        {
            BaseUrl = configurationSection["BaseUrl"] ?? string.Empty,
            Model = configurationSection["Model"] ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(options.BaseUrl) || string.IsNullOrWhiteSpace(options.Model))
            throw new InvalidOperationException("Ollama BaseUrl and Model must be configured.");

        services.AddSingleton(options);
        services.AddSingleton(new OllamaSharp.OllamaApiClient(new Uri(options.BaseUrl), options.Model));
        services.AddSingleton<ITextGenerationService, OllamaTextGenerationService>();

        return services;
    }

    private static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");

        services.AddScoped<IPromptJobQueue, PromptJobQueue>();

        services.AddMassTransit(configurator =>
        {
            configurator.AddEntityFrameworkOutbox<ApplicationDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
            });

            configurator.UsingRabbitMq((context, rabbitMq) =>
            {
                rabbitMq.Host(new Uri(connectionString));
            });
        });

        return services;
    }
}
