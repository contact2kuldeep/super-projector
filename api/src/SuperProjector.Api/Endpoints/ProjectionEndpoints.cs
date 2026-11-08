using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using SuperProjector.Api.Domain;

namespace SuperProjector.Api.Endpoints;

/// <summary>
/// Maps the projection HTTP endpoint. This is the thin "web" layer: it handles
/// HTTP concerns (JSON in, status codes out) and hands the real work to the
/// pure classes in the Domain folder.
/// </summary>
public static class ProjectionEndpoints
{
    public static IEndpointRouteBuilder MapProjectionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projection", Project)
            .WithName("CreateProjection")
            .WithSummary("Project a super balance to retirement age")
            .WithDescription("Returns the projected balance at age 67 using the configured assumptions.");

        return app;
    }

    // ASP.NET binds the JSON body to 'request' and injects the configured
    // assumptions via IOptions. The Results<...> return type tells OpenAPI
    // exactly which responses are possible (200 or 400).
    private static Results<Ok<ProjectionResponse>, ValidationProblem> Project(
        ProjectionRequest request,
        IOptions<ProjectionAssumptions> assumptions)
    {
        var errors = ProjectionRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            // 400 Bad Request with a standard "problem details" body listing each error.
            return TypedResults.ValidationProblem(errors);
        }

        var response = ProjectionCalculator.Calculate(request, assumptions.Value);
        return TypedResults.Ok(response);
    }
}
