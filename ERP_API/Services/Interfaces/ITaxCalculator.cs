namespace ERP_API.Services.Interfaces;


public interface ITaxCalculator
{

    decimal CalculateTax(decimal amount, TaxType taxType = TaxType.IVA);

  
    decimal CalculateCompositeTax(decimal amount, params TaxType[] taxTypes);

    decimal GetTaxRate(TaxType taxType);

    decimal CalculateSubtotalFromTotal(decimal totalWithTax, TaxType taxType = TaxType.IVA);
}


public enum TaxType
{

    IVA = 1,

    ISR = 2,


    None = 0
}