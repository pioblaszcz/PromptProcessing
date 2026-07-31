using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PromptProcessing.Api.ExceptionHandling;
using PromptProcessing.Api.Pagination;
using PromptProcessing.Application;
using PromptProcessing.Infrastructure;
using PromptProcessing.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidPromptJobsCursorExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<PromptJobsCursorCodec>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var frontendUrl = builder.Configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend base URL is missing.");

        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await databaseContext.Database.MigrateAsync();
    return;
}

app.UseExceptionHandler();

if (builder.Configuration.GetValue<bool?>("HttpsRedirection:Enabled") ?? true)
    app.UseHttpsRedirection();

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
