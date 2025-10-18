namespace ERP_API.Common.Exceptions;


public class ConcurrencyException : DomainException
{
    public string EntityName { get; }
    public Guid EntityId { get; }

    public ConcurrencyException(string entityName, Guid entityId)
        : base($"El {entityName} con ID {entityId} fue modificado por otro usuario. Por favor, recargue los datos e intente nuevamente.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}