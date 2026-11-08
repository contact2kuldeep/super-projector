namespace SuperProjector.Api.Domain;

/// <summary>
/// The details a member enters to get a projection.
/// This is the JSON body of <c>POST /api/projection</c>.
/// </summary>
/// <param name="Age">The member's current age in whole years (18 to 66).</param>
/// <param name="CurrentBalance">Today's super balance in dollars.</param>
/// <param name="AnnualSalary">Gross annual salary in dollars, before tax.</param>
/// <param name="ContributionRatePercent">
/// Percentage of salary paid into super each year, e.g. 12 for 12%.
/// </param>
public record ProjectionRequest(
    int Age,
    decimal CurrentBalance,
    decimal AnnualSalary,
    decimal ContributionRatePercent);
