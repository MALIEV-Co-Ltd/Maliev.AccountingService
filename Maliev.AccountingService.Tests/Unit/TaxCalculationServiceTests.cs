using Maliev.AccountingService.Api.Services;
using Xunit;

namespace Maliev.AccountingService.Tests.Unit;

public class TaxCalculationServiceTests
{
    private readonly TaxCalculationService _service;

    public TaxCalculationServiceTests()
    {
        _service = new TaxCalculationService();
    }

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(100, 20, 20)]
    [InlineData(100, 0, 0)]
    [InlineData(123.45, 10, 12.35)] // 12.345 rounded to 12.35
    [InlineData(123.44, 10, 12.34)] // 12.344 rounded to 12.34
    public void CalculateVat_ShouldReturnCorrectAmount(decimal subtotal, decimal rate, decimal expected)
    {
        // Act
        var result = _service.CalculateVat(subtotal, rate);

        // Assert
        Assert.Equal(expected, result);
    }
}
