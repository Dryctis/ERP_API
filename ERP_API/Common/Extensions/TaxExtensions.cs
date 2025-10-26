using ERP_API.Services.Interfaces;

namespace ERP_API.Common.Extensions;


public static class TaxExtensions
{
 
    public static decimal WithIVA(this decimal amount, ITaxCalculator calculator)
    {
        var tax = calculator.CalculateTax(amount, TaxType.IVA);
        return amount + tax;
    }

    public static decimal IVAAmount(this decimal amount, ITaxCalculator calculator)
    {
        return calculator.CalculateTax(amount, TaxType.IVA);
    }

    public static decimal WithoutIVA(this decimal totalWithTax, ITaxCalculator calculator)
    {
        return calculator.CalculateSubtotalFromTotal(totalWithTax, TaxType.IVA);
    }
}