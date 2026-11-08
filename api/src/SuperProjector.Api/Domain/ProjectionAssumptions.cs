namespace SuperProjector.Api.Domain;

/// <summary>
/// The economic assumptions behind a projection.
/// </summary>
/// <remarks>
/// Values are loaded from the "Projection" section of appsettings.json, so each
/// environment can change them without a code change. The defaults below apply
/// when a setting is missing from configuration.
/// </remarks>
public class ProjectionAssumptions
{
    /// <summary>The configuration section these values are read from.</summary>
    public const string SectionName = "Projection";

    /// <summary>Yearly investment return after fees and earnings tax, as a percentage.</summary>
    public decimal NetReturnPercent { get; set; } = 6.5m;

    /// <summary>How much the member's salary grows each year, as a percentage.</summary>
    public decimal WageGrowthPercent { get; set; } = 3.0m;

    /// <summary>Tax taken from each contribution as it enters the fund, as a percentage.</summary>
    public decimal ContributionsTaxPercent { get; set; } = 15m;

    /// <summary>The age at which the projection stops.</summary>
    public int RetirementAge { get; set; } = 67;
}
