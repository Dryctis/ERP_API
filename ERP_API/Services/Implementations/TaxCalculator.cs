using ERP_API.Common.Settings;
using ERP_API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ERP_API.Services.Implementations;


public class TaxCalculator : ITaxCalculator
{
    private readonly TaxSettings _settings;
    private readonly ILogger<TaxCalculator> _logger;

    public TaxCalculator(IOptions<TaxSettings> settings, ILogger<TaxCalculator> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public decimal CalculateTax(decimal amount, TaxType taxType = TaxType.IVA)
    {
        if (amount < 0)
        {
            _logger.LogWarning("Intento de calcular impuesto sobre monto negativo: {Amount}", amount);
            return 0;
        }

        if (taxType == TaxType.None)
            return 0;

        var rate = GetTaxRate(taxType);
        var tax = Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);

        _logger.LogDebug(
            "Impuesto calculado. Tipo: {TaxType}, Base: {Amount}, Tasa: {Rate}, Impuesto: {Tax}",
            taxType, amount, rate, tax
        );

        return tax;
    }

    public decimal CalculateCompositeTax(decimal amount, params TaxType[] taxTypes)
    {
        if (taxTypes == null || taxTypes.Length == 0)
            return 0;

        decimal totalTax = 0;

        foreach (var taxType in taxTypes.Where(t => t != TaxType.None))
        {
            totalTax += CalculateTax(amount, taxType);
        }

        _logger.LogDebug(
            "Impuesto compuesto calculado. Base: {Amount}, Tipos: {TaxTypes}, Total: {TotalTax}",
            amount, string.Join(", ", taxTypes), totalTax
        );

        return totalTax;
    }

    public decimal GetTaxRate(TaxType taxType)
    {
        return taxType switch
        {
            TaxType.IVA => _settings.IvaRate,
            TaxType.ISR => _settings.IsrRate,
            TaxType.None => 0m,
            _ => throw new ArgumentException($"Tipo de impuesto no soportado: {taxType}", nameof(taxType))
        };
    }

    public decimal CalculateSubtotalFromTotal(decimal totalWithTax, TaxType taxType = TaxType.IVA)
    {
        if (totalWithTax < 0)
        {
            _logger.LogWarning("Intento de calcular subtotal desde total negativo: {Total}", totalWithTax);
            return 0;
        }

        if (taxType == TaxType.None)
            return totalWithTax;

        var rate = GetTaxRate(taxType);
        var subtotal = Math.Round(totalWithTax / (1 + rate), 2, MidpointRounding.AwayFromZero);

        _logger.LogDebug(
            "Subtotal calculado desde total. Total: {Total}, Tipo: {TaxType}, Subtotal: {Subtotal}",
            totalWithTax, taxType, subtotal
        );

        return subtotal;
    }
}