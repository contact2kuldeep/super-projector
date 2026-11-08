namespace SuperProjector.Api.Domain;

/// <summary>
/// Checks a <see cref="ProjectionRequest"/> before it is calculated.
/// </summary>
/// <remarks>
/// Like the calculator, this is plain C# with no framework dependencies, so it
/// can be unit tested directly. It collects every problem rather than stopping
/// at the first, so the user can fix them all at once.
/// </remarks>
public static class ProjectionRequestValidator
{
    public const int MinAge = 18;
    public const int MaxAge = 66;
    public const decimal MaxContributionRatePercent = 50m;

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <returns>
    /// A map of field name to error messages, in the shape ASP.NET's
    /// <c>ValidationProblem</c> expects. Empty when the request is valid.
    /// </returns>
    public static Dictionary<string, string[]> Validate(ProjectionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Age < MinAge || request.Age > MaxAge)
        {
            errors["age"] = [$"Age must be between {MinAge} and {MaxAge}."];
        }

        if (request.CurrentBalance < 0)
        {
            errors["currentBalance"] = ["Current balance cannot be negative."];
        }

        if (request.AnnualSalary < 0)
        {
            errors["annualSalary"] = ["Annual salary cannot be negative."];
        }

        if (request.ContributionRatePercent < 0 || request.ContributionRatePercent > MaxContributionRatePercent)
        {
            errors["contributionRatePercent"] =
                [$"Contribution rate must be between 0 and {MaxContributionRatePercent}%."];
        }

        return errors;
    }
}
