using SuperProjector.Api.Domain;

namespace SuperProjector.Api.Tests.Unit;

/// <summary>
/// Unit tests for request validation. Boundary values (the first and last
/// allowed numbers) are where off-by-one mistakes hide, so we test both sides.
/// </summary>
public class ProjectionRequestValidatorTests
{
    // A known-good request. Each test changes just one field from this.
    private static readonly ProjectionRequest ValidRequest = new(
        Age: 40, CurrentBalance: 100_000m, AnnualSalary: 90_000m, ContributionRatePercent: 12m);

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(67)]
    public void Validate_AgeOutsideRange_ReturnsAgeError(int age)
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { Age = age });

        Assert.Equal(["Age must be between 18 and 66."], errors["age"]);
    }

    [Theory]
    [InlineData(18)]
    [InlineData(66)]
    public void Validate_AgeAtBoundary_ReturnsNoErrors(int age)
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { Age = age });

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NegativeBalance_ReturnsBalanceError()
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { CurrentBalance = -0.01m });

        Assert.Equal(["Current balance cannot be negative."], errors["currentBalance"]);
    }

    [Fact]
    public void Validate_NegativeSalary_ReturnsSalaryError()
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { AnnualSalary = -1m });

        Assert.Equal(["Annual salary cannot be negative."], errors["annualSalary"]);
    }

    [Fact]
    public void Validate_ZeroBalanceAndZeroSalary_ReturnsNoErrors()
    {
        var errors = ProjectionRequestValidator.Validate(
            ValidRequest with { CurrentBalance = 0m, AnnualSalary = 0m });

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public void Validate_ContributionRateOutsideRange_ReturnsRateError(int ratePercent)
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { ContributionRatePercent = ratePercent });

        Assert.Equal(["Contribution rate must be between 0 and 50%."], errors["contributionRatePercent"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    public void Validate_ContributionRateAtBoundary_ReturnsNoErrors(int ratePercent)
    {
        var errors = ProjectionRequestValidator.Validate(ValidRequest with { ContributionRatePercent = ratePercent });

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_SeveralInvalidFields_ReturnsAllErrors()
    {
        var request = new ProjectionRequest(Age: 70, CurrentBalance: -1m, AnnualSalary: -1m, ContributionRatePercent: 60m);

        var errors = ProjectionRequestValidator.Validate(request);

        Assert.Equal(["age", "currentBalance", "annualSalary", "contributionRatePercent"], errors.Keys);
    }
}
