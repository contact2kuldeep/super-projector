using SuperProjector.Api.Domain;

namespace SuperProjector.Api.Tests.Unit;

/// <summary>
/// Unit tests for the pure projection maths. No web host, no configuration:
/// we call the calculator directly, so these run in milliseconds.
/// </summary>
public class ProjectionCalculatorTests
{
    // A fresh object each time means the defaults from ProjectionAssumptions
    // (6.5% return, 3% wage growth, 15% tax, retire at 67).
    private static ProjectionAssumptions DefaultAssumptions() => new();

    [Fact]
    public void Calculate_KnownInputsWithDefaultAssumptions_ReturnsExpectedValues()
    {
        var request = new ProjectionRequest(
            Age: 40, CurrentBalance: 150_000m, AnnualSalary: 110_000m, ContributionRatePercent: 12m);

        var result = ProjectionCalculator.Calculate(request, DefaultAssumptions());

        Assert.Equal(27, result.YearsToRetirement);
        Assert.Equal(1_932_437.34m, result.ProjectedBalance);
        Assert.Equal(456_762.09m, result.TotalContributions);
    }

    [Fact]
    public void Calculate_KnownInputs_TotalsAddUpToProjectedBalance()
    {
        // Every dollar in the final balance is either the starting balance, a
        // contribution or an earning. Allow 1 cent for rounding each total.
        var request = new ProjectionRequest(40, 150_000m, 110_000m, 12m);

        var result = ProjectionCalculator.Calculate(request, DefaultAssumptions());

        decimal sumOfParts = request.CurrentBalance + result.TotalContributions + result.TotalInvestmentEarnings;
        Assert.InRange(result.ProjectedBalance - sumOfParts, -0.01m, 0.01m);
    }

    [Theory]
    [InlineData(18)]
    [InlineData(40)]
    [InlineData(66)]
    public void Calculate_ZeroSalaryAndZeroBalance_ReturnsZeroBalance(int age)
    {
        var request = new ProjectionRequest(age, CurrentBalance: 0m, AnnualSalary: 0m, ContributionRatePercent: 12m);

        var result = ProjectionCalculator.Calculate(request, DefaultAssumptions());

        Assert.Equal(0m, result.ProjectedBalance);
        Assert.Equal(0m, result.TotalContributions);
        Assert.Equal(0m, result.TotalInvestmentEarnings);
    }

    [Theory]
    [InlineData(30, 50_000, 90_000, 12)]
    [InlineData(60, 0, 80_000, 10)]
    [InlineData(66, 250_000, 120_000, 15)]
    public void Calculate_ZeroReturnAndZeroWageGrowth_ReturnsStartPlusTaxedContributions(
        int age, int currentBalance, int annualSalary, int ratePercent)
    {
        // With no growth the maths is simple enough to work out by hand:
        // each year adds the same contribution, less 15% tax.
        var assumptions = new ProjectionAssumptions { NetReturnPercent = 0m, WageGrowthPercent = 0m };
        var request = new ProjectionRequest(age, currentBalance, annualSalary, ratePercent);
        int years = 67 - age;

        var result = ProjectionCalculator.Calculate(request, assumptions);

        decimal expected = currentBalance + (annualSalary * (ratePercent / 100m) * 0.85m * years);
        Assert.Equal(expected, result.ProjectedBalance);
        Assert.Equal(0m, result.TotalInvestmentEarnings);
    }

    [Fact]
    public void Calculate_Age66_ReturnsOneYearToRetirement()
    {
        var request = new ProjectionRequest(66, 100_000m, 80_000m, 12m);

        var result = ProjectionCalculator.Calculate(request, DefaultAssumptions());

        Assert.Equal(1, result.YearsToRetirement);
        Assert.Equal(67, result.RetirementAge);
    }

    [Theory]
    [InlineData(0, 15)]
    [InlineData(15, 30)]
    [InlineData(30, 47)]
    public void Calculate_HigherContributionsTax_ReturnsLowerBalance(int lowerTaxPercent, int higherTaxPercent)
    {
        var request = new ProjectionRequest(40, 150_000m, 110_000m, 12m);

        var lowerTax = ProjectionCalculator.Calculate(
            request, new ProjectionAssumptions { ContributionsTaxPercent = lowerTaxPercent });
        var higherTax = ProjectionCalculator.Calculate(
            request, new ProjectionAssumptions { ContributionsTaxPercent = higherTaxPercent });

        Assert.True(higherTax.ProjectedBalance < lowerTax.ProjectedBalance,
            $"Expected {higherTaxPercent}% tax to give less than {lowerTaxPercent}% tax.");
    }

    [Fact]
    public void Calculate_AnyRequest_EchoesAssumptionsAndDisclaimer()
    {
        var assumptions = new ProjectionAssumptions { NetReturnPercent = 5m };
        var request = new ProjectionRequest(40, 150_000m, 110_000m, 12m);

        var result = ProjectionCalculator.Calculate(request, assumptions);

        Assert.Same(assumptions, result.Assumptions);
        Assert.Equal("Illustrative only. Not financial advice.", result.Disclaimer);
    }

    [Theory]
    [InlineData(67)]
    [InlineData(80)]
    public void Calculate_AgeAtOrPastRetirement_ThrowsArgumentException(int age)
    {
        // The validator stops this reaching the calculator via the API, but the
        // calculator still protects itself if it is called some other way.
        var request = new ProjectionRequest(age, 100_000m, 80_000m, 12m);

        Assert.Throws<ArgumentException>(() => ProjectionCalculator.Calculate(request, DefaultAssumptions()));
    }
}
