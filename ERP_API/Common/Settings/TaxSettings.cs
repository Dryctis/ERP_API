namespace ERP_API.Common.Settings;


public class TaxSettings
{
    public const string SectionName = "Tax";


    public decimal IvaRate { get; set; } = 0.12m;

 
    public decimal IsrRate { get; set; } = 0.05m;


    public bool PricesIncludeTax { get; set; } = false;

    public int DecimalPlaces { get; set; } = 2;

    public string RoundingMode { get; set; } = "AwayFromZero";

 
    public void Validate()
    {
        if (IvaRate < 0 || IvaRate > 1)
            throw new InvalidOperationException($"IVA Rate debe estar entre 0 y 1. Valor actual: {IvaRate}");

        if (IsrRate < 0 || IsrRate > 1)
            throw new InvalidOperationException($"ISR Rate debe estar entre 0 y 1. Valor actual: {IsrRate}");

        if (DecimalPlaces < 0 || DecimalPlaces > 10)
            throw new InvalidOperationException($"DecimalPlaces debe estar entre 0 y 10. Valor actual: {DecimalPlaces}");
    }
}