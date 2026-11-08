namespace SuperProjector.Api.Domain;

/// <summary>
/// Projects a super balance forward to retirement age.
/// </summary>
/// <remarks>
/// This class is "pure": it has no dependencies on ASP.NET, configuration,
/// the clock or a database. The same inputs always give the same output,
/// which makes it simple to unit test.
/// </remarks>
public static class ProjectionCalculator
{
    /// <summary>The disclaimer included with every projection.</summary>
    public const string Disclaimer = "Illustrative only. Not financial advice.";

    /// <summary>
    /// Runs the projection one year at a time, compounding annually.
    /// </summary>
    /// <param name="request">The member's details. Assumed to be valid already.</param>
    /// <param name="assumptions">Return, wage growth, tax and retirement age to use.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the member is already at or past retirement age.
    /// </exception>
    public static ProjectionResponse Calculate(ProjectionRequest request, ProjectionAssumptions assumptions)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(assumptions);

        int yearsToRetirement = assumptions.RetirementAge - request.Age;
        if (yearsToRetirement <= 0)
        {
            throw new ArgumentException(
                $"Age must be below the retirement age of {assumptions.RetirementAge}.", nameof(request));
        }

        // Convert percentages (e.g. 6.5) to rates (e.g. 0.065) once, up front.
        decimal contributionRate = request.ContributionRatePercent / 100m;
        decimal contributionsTaxRate = assumptions.ContributionsTaxPercent / 100m;
        decimal netReturnRate = assumptions.NetReturnPercent / 100m;
        decimal wageGrowthRate = assumptions.WageGrowthPercent / 100m;

        // Running totals. We keep full precision here and only round at the end,
        // so rounding errors don't build up over decades of compounding.
        decimal balance = request.CurrentBalance;
        decimal salary = request.AnnualSalary;
        decimal totalContributions = 0m;
        decimal totalEarnings = 0m;

        for (int year = 1; year <= yearsToRetirement; year++)
        {
            // 1. Contributions for the year, less contributions tax.
            decimal grossContribution = salary * contributionRate;
            decimal netContribution = grossContribution * (1m - contributionsTaxRate);
            balance += netContribution;
            totalContributions += netContribution;

            // 2. Investment return on the whole balance (including this year's
            //    contributions). A simplification: real contributions arrive
            //    throughout the year, not all at the start.
            decimal earnings = balance * netReturnRate;
            balance += earnings;
            totalEarnings += earnings;

            // 3. Salary grows ready for next year.
            salary *= 1m + wageGrowthRate;
        }

        return new ProjectionResponse(
            YearsToRetirement: yearsToRetirement,
            RetirementAge: assumptions.RetirementAge,
            ProjectedBalance: RoundMoney(balance),
            TotalContributions: RoundMoney(totalContributions),
            TotalInvestmentEarnings: RoundMoney(totalEarnings),
            Assumptions: assumptions,
            Disclaimer: Disclaimer);
    }

    // Round to cents. AwayFromZero means 0.005 rounds up to 0.01, the way most
    // people expect; .NET's default (banker's rounding) would round it to 0.00.
    private static decimal RoundMoney(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
