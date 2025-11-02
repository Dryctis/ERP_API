namespace ERP_API.Common.Configuration;

public static class BusinessConstants
{
 
    public static class Tax
    {
 
        public const decimal StandardRate = 0.12m;

        public static decimal Calculate(decimal subtotal)
            => Math.Round(subtotal * StandardRate, 2, MidpointRounding.AwayFromZero);
    }

    public static class Invoicing
    {
        public const int DefaultPaymentTermDays = 30;
        public const int MaxPaymentTermDays = 365;
        public const string DefaultPaymentTerms = "Net 30";
        public const string InvoiceNumberPrefix = "INV";

  
        public const int MaxInvoiceItems = 100;
    }


    public static class PurchaseOrders
    {
        public const int DefaultDeliveryDays = 7;
        public const string OrderNumberPrefix = "PO";

  
        public const int MaxPurchaseOrderItems = 100;

        public const int MaxQuantityPerItem = 100000;
    }

    public static class Orders
    {
    
        public const int MaxOrderItems = 100;

   
        public const int MinOrderItems = 1;

        public const int MaxProductQuantityPerItem = 10000;
    }

   
    public static class Inventory
    {
      
        public const int DefaultLowStockThreshold = 10;

       
        public const int ReorderMultiplier = 2;

       
        public const int MinimumStock = 0;

       
        public const int MaximumStock = 1000000;
    }

    public static class Pricing
    {

        public const decimal MinimumPrice = 0.01m;

        public const decimal MaximumPrice = 999999.99m;

        public const decimal MaxDiscountPercentage = 1.0m;
    }

    public static class StringLengths
    {
        public const int MaxProductNameLength = 200;
        public const int MaxNameLength = 200;
        public const int MaxSkuLength = 50;
        public const int MaxNotesLength = 1000;
    }

    public static class Pagination
    {

        public const int DefaultPageSize = 20;

        public const int MaxPageSize = 100;

        public const int DefaultPage = 1;
    }
}