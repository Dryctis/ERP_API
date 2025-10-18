namespace ERP_API.Common.Exceptions;


public class StockInsuficienteException : DomainException
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Disponible { get; }
    public int Requerido { get; }

    public StockInsuficienteException(Guid productId, string productName, int disponible, int requerido)
        : base($"Stock insuficiente para '{productName}'. Disponible: {disponible}, Requerido: {requerido}")
    {
        ProductId = productId;
        ProductName = productName;
        Disponible = disponible;
        Requerido = requerido;
    }
}