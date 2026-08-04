using MassTransit;
using PromptProcessing.Application.Abstractions.AI;
using PromptProcessing.Infrastructure;
using PromptProcessing.Worker;
using PromptProcessing.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructurePersistence(builder.Configuration);
builder.Services.AddOllama(builder.Configuration);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<ProcessPromptJobConsumer>();

    configurator.UsingRabbitMq((context, rabbitMq) =>
    {
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("RabbitMQ connection string is missing.");

        rabbitMq.Host(new Uri(rabbitMqConnectionString));

        rabbitMq.ReceiveEndpoint("prompt-processing", endpoint =>
        {
            endpoint.UseMessageRetry(retry =>
            {
                retry.Handle<TransientTextGenerationException>();
                retry.Intervals(PromptGenerationRetryPolicy.RetryIntervals);
            });

            endpoint.ConfigureConsumer<ProcessPromptJobConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
