using CBS.BI.Api.Exceptions;
using CBS.BI.Api.Security;
using CBS.BI.Api.Security.Development;
using CBS.BI.Api.Development;
using CBS.BI.Application.Analytics.Abstractions;
using CBS.BI.Application.Analytics.SemanticKnowledge;
using CBS.BI.Application.Analytics.SemanticRetrieval;
using CBS.BI.Application.Analytics.UseCases.DeleteAnalyticsDashboard;
using CBS.BI.Application.Analytics.UseCases.DeleteSavedAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteNaturalLanguageAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.ExecuteVisualQuery;
using CBS.BI.Application.Analytics.UseCases.GenerateAnalyticsQuery;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboards;
using CBS.BI.Application.Analytics.UseCases.GetAnalyticsDashboardById;
using CBS.BI.Application.Analytics.UseCases.GetSavedAnalyticsQueries;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsDashboard;
using CBS.BI.Application.Analytics.UseCases.SaveAnalyticsQuery;
using CBS.BI.Application.Analytics.Validation;
using CBS.BI.Application.Security.Abstractions;
using CBS.BI.Application.Security.Authorization;
using CBS.BI.Infrastructure.Analytics;
using CBS.BI.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure request timeouts
var analyticsTimeoutSeconds = builder.Configuration.GetValue<int>("Analytics:RequestTimeoutSeconds", 55);
if (analyticsTimeoutSeconds <= 0)
{
    throw new InvalidOperationException("Analytics:RequestTimeoutSeconds must be a positive integer.");
}

builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy(
   "AnalyticsExecution",
        new RequestTimeoutPolicy
        {
Timeout = TimeSpan.FromSeconds(analyticsTimeoutSeconds),
    TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
      WriteTimeoutResponse = async context =>
         {
           // Diagnostic logging
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
     var logger = loggerFactory.CreateLogger("Analytics.Timeout");
            logger.LogInformation("Analytics timeout response callback entered. ResponseHasStarted={HasStarted}, StatusCode={StatusCode}", 
         context.Response.HasStarted, context.Response.StatusCode);
                
    // Set status code and content type
 context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
  context.Response.ContentType = "application/problem+json";

     var problemDetails = new ProblemDetails
          {
    Type = "analytics-request-timeout",
      Title = "Analytics request timed out",
    Status = StatusCodes.Status504GatewayTimeout,
  Detail = "The analytics request exceeded the configured execution time limit."
    };
        
    // Serialize ProblemDetails explicitly using System.Text.Json to avoid WriteAsJsonAsync issues with cancelled tokens
 var json = JsonSerializer.Serialize(problemDetails);
 
 // Write serialized JSON directly to response stream
 await context.Response.WriteAsync(json);
       
       logger.LogInformation("Analytics timeout response callback completed.");
         }
 });
});

// Add CORS policy
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
  {
    policy.WithOrigins(allowedOrigins!)
.AllowAnyMethod()
         .AllowAnyHeader();
    });
});

// Configure GoogleCloud options
builder.Services.Configure<GoogleCloudOptions>(
    builder.Configuration.GetSection(GoogleCloudOptions.SectionName));

// Register analytics dependencies
builder.Services.AddScoped<IAnalyticsQueryExecutor, BigQueryAnalyticsQueryExecutor>();
builder.Services.AddScoped<IQueryCostEstimator, BigQueryQueryCostEstimator>();
builder.Services.AddScoped<IAnalyticsQuerySafetyValidator, ReadOnlyAnalyticsQuerySafetyValidator>();
builder.Services.AddScoped<IAnalyticsQueryGenerator, GeminiAnalyticsQueryGenerator>();
builder.Services.AddScoped<IAnalyticsMetadataCatalog, BigQueryAnalyticsMetadataCatalog>();
builder.Services.AddScoped<ExecuteAnalyticsQueryUseCase>();
builder.Services.AddScoped<GenerateAnalyticsQueryUseCase>();
builder.Services.AddScoped<ExecuteNaturalLanguageAnalyticsQueryUseCase>();

// Register visual query dependencies
builder.Services.AddSingleton<IVisualQueryCatalog, VisualQueryCatalog>();
builder.Services.AddScoped<ExecuteVisualQueryUseCase>();

// Register saved queries and dashboards (POC implementations)
builder.Services.AddSingleton<ISavedAnalyticsQueryStore, InMemorySavedAnalyticsQueryStore>();
builder.Services.AddSingleton<IAnalyticsDashboardStore, InMemoryAnalyticsDashboardStore>();
builder.Services.AddScoped<SaveAnalyticsQueryUseCase>();
builder.Services.AddScoped<GetSavedAnalyticsQueriesUseCase>();
builder.Services.AddScoped<DeleteSavedAnalyticsQueryUseCase>();
builder.Services.AddScoped<SaveAnalyticsDashboardUseCase>();
builder.Services.AddScoped<GetAnalyticsDashboardsUseCase>();
builder.Services.AddScoped<GetAnalyticsDashboardByIdUseCase>();
builder.Services.AddScoped<DeleteAnalyticsDashboardUseCase>();

// Register semantic retrieval dependencies
builder.Services.AddScoped<MetadataAnalyticsSemanticKnowledgeSource>();
builder.Services.AddScoped<BusinessDictionaryAnalyticsSemanticKnowledgeSource>();
builder.Services.AddScoped<IAnalyticsSemanticKnowledgeSource, CompositeAnalyticsSemanticKnowledgeSource>();
builder.Services.AddScoped<IAnalyticsSemanticRetriever, LexicalAnalyticsSemanticRetriever>();

// Register security/current user context dependencies
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddScoped<IAnalyticsAuthorizationService, AnalyticsAuthorizationService>();

// Register Development authentication for local testing only
if (builder.Environment.IsDevelopment())
{
 builder.Services
        .AddAuthentication(DevelopmentAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
       DevelopmentAuthenticationHandler.SchemeName, null);

    // Register Development/POC semantic context provider
    builder.Services.AddScoped<IAnalyticsSemanticContextProvider, DevelopmentAnalyticsSemanticContextProvider>();
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add exception handling middleware
app.UseExceptionHandler();

// Use request timeouts middleware
app.UseRequestTimeouts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.UseAuthentication();
 
    app.MapDevelopmentAnalyticsEndpoints();
}

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowReactDev");

app.MapControllers();

app.Run();
