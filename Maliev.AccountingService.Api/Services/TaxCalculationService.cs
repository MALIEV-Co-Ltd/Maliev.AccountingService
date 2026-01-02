namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for calculating taxes based on SUBTOTAL and rates
/// </summary>
public interface ITaxCalculationService
{
    /// <summary>
    /// Calculates the VAT amount for a given subtotal and rate.
    /// </summary>
    decimal CalculateVat(decimal subtotal, decimal rate);
}

/// <summary>
/// Implementation of ITaxCalculationService
/// </summary>
public class TaxCalculationService : ITaxCalculationService
{
    /// <inheritdoc />
    public decimal CalculateVat(decimal subtotal, decimal rate)
    {
        return Math.Round(subtotal * rate / 100m, 2);
    }
}
