using Example.Api.Config.ApiVersioning;
using Example.Api.Config.NotifierService;
using Example.Api.Config.Otel;
using Example.Api.Config.ProblemDetails;
using Example.Api.Config.RequestPipeline;
using Example.Api.Features;
using Example.Api.Features.Orders;
using Example.Api.Features.Products;
using Example.Api.Infrastructure.Messaging;
using Example.Api.Infrastructure.Persistence;
using Example.SharedKernel.Services.MessagingService;

var builder = WebApplication.CreateBuilder(args);

// Adding open telemetry configuration
builder.AddOtelLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// adding problem details services
builder.Services.AddExtendedProblemDetails();

// adding api versioning services
builder.Services.AddApiVersioningExtensions();

// adding database services
builder.Services.AddDatabaseServices(builder.Configuration);

// adding the notifier service and its handlers
builder.Services.AddNotifier();

// adding the api handlers + pipelines
builder.Services.AddApiHandlers();

// adding the feature specific services
builder.Services.AddFeatures();

// adding the consumer services
builder.Services.AddConsumerServices(builder.Configuration);
builder.UseServiceBusListeners();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapProductEndpoints();
app.MapOrderEndpoints();

app.UseHttpsRedirection();

var dbInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await dbInitializer.InitializeAsync();


app.Run();

