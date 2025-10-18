namespace ERP_API.Common.Exceptions;


public class ProductoNoEncontradoException : DomainException
{
    public Guid ProductId { get; }

    public ProductoNoEncontradoException(Guid productId)
        : base($"Producto con ID {productId} no encontrado")
    {
        ProductId = productId;
    }
}