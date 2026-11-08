namespace SuperProjector.Api.Domain;

/// <summary>
/// The result of a projection, returned as JSON to the web app.
/// All money values are rounded to 2 decimal places (cents).
/// </summary>
/// <param name="YearsToRetirement">Number of years projected (retirement age minus current age).</param>
/// <param name="RetirementAge">The age the projection runs to.</param>
/// <param name="ProjectedBalance">Estimated balance at retirement age.</param>
/// <param name="TotalContributions">Sum of all contributions after contributions tax.</param>
/// <param name="TotalInvestmentEarnings">Sum of all investment returns credited.</param>
/// <param name="Assumptions">The assumptions used, echoed back so the result can be explained.</param>
/// <param name="Disclaimer">A short reminder that this is not advice.</param>
public record ProjectionResponse(
    int YearsToRetirement,
    int RetirementAge,
    decimal ProjectedBalance,
    decimal TotalContributions,
    decimal TotalInvestmentEarnings,
    ProjectionAssumptions Assumptions,
    string Disclaimer);
