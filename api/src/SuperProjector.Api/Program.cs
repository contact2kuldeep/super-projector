// Program.cs is the entry point. In a minimal API it does two jobs:
//   1. Register services (dependency injection) on the builder.
//   2. Build the app and configure the HTTP pipeline and endpoints.

using Scalar.AspNetCore;
using SuperProjector.Api.Domain;
using SuperProjector.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ---- 1. Services -----------------------------------------------------------

// Bind the "Projection" section of appsettings.json to ProjectionAssumptions.
// Endpoints receive it as IOptions<ProjectionAssumptions>.
builder.Services.Configure<ProjectionAssumptions>(
    builder.Configuration.GetSection(ProjectionAssumptions.SectionName));

// Generates the OpenAPI (Swagger) document describing our endpoints.
builder.Services.AddOpenApi();

// Built-in health checks. With no checks registered it simply reports
// "Healthy" while the app is running, which is enough for App Service.
builder.Services.AddHealthChecks();

// CORS lets the React app (served from a different origin) call this API.
// Allowed origins come from config so each environment can set its own.
const string WebAppCorsPolicy = "WebApp";
string[] allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"]; // Vite dev server default
builder.Services.AddCors(options =>
    options.AddPolicy(WebAppCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ---- 2. Pipeline and endpoints ---------------------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // /openapi/v1.json: the raw OpenAPI document.
    app.MapOpenApi();
    // /scalar: an interactive UI for trying the API in the browser.
    app.MapScalarApiReference();
}

// Note: no UseHttpsRedirection. Locally we run on plain http; in Azure,
// App Service terminates HTTPS and will be set to HTTPS-only in Bicep.

app.UseCors(WebAppCorsPolicy);

// GET /health -> 200 { "status": "Healthy" }
// The custom writer returns JSON instead of the default plain-text body.
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = (context, report) =>
        context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() })
});

// POST /api/projection (see Endpoints/ProjectionEndpoints.cs)
app.MapProjectionEndpoints();

app.Run();
