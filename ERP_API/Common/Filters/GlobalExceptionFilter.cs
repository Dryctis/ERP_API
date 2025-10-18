using ERP_API.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Common.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
       
        if (context.Exception is DbUpdateConcurrencyException concurrencyEx)
        {
            _logger.LogWarning(concurrencyEx, "Conflicto de concurrencia detectado");

            context.Result = new ConflictObjectResult(new
            {
                error = "Los datos fueron modificados por otro usuario. Por favor, recargue e intente nuevamente.",
                type = "ConcurrencyConflict"
            });
            context.ExceptionHandled = true;
            return;
        }

        
        if (context.Exception is StockInsuficienteException stockEx)
        {
            _logger.LogWarning(
                stockEx,
                "Stock insuficiente. ProductId: {ProductId}, Disponible: {Disponible}, Requerido: {Requerido}",
                stockEx.ProductId,
                stockEx.Disponible,
                stockEx.Requerido
            );

            context.Result = new ConflictObjectResult(new
            {
                error = stockEx.Message,
                type = "StockInsuficiente",
                productId = stockEx.ProductId,
                productName = stockEx.ProductName,
                disponible = stockEx.Disponible,
                requerido = stockEx.Requerido
            });
            context.ExceptionHandled = true;
            return;
        }

        
        if (context.Exception is ProductoNoEncontradoException prodEx)
        {
            _logger.LogWarning(prodEx, "Producto no encontrado: {ProductId}", prodEx.ProductId);

            context.Result = new NotFoundObjectResult(new
            {
                error = prodEx.Message,
                type = "ProductoNoEncontrado",
                productId = prodEx.ProductId
            });
            context.ExceptionHandled = true;
            return;
        }

       
        if (context.Exception is ConcurrencyException concEx)
        {
            _logger.LogWarning(
                concEx,
                "Conflicto de concurrencia. Entity: {EntityName}, Id: {EntityId}",
                concEx.EntityName,
                concEx.EntityId
            );

            context.Result = new ConflictObjectResult(new
            {
                error = concEx.Message,
                type = "ConcurrencyConflict",
                entityName = concEx.EntityName,
                entityId = concEx.EntityId
            });
            context.ExceptionHandled = true;
            return;
        }

        
        if (context.Exception is DomainException domainEx)
        {
            _logger.LogWarning(domainEx, "Error de dominio: {Message}", domainEx.Message);

            context.Result = new BadRequestObjectResult(new
            {
                error = domainEx.Message,
                type = "DomainError"
            });
            context.ExceptionHandled = true;
            return;
        }

       
        _logger.LogError(context.Exception, "Error no controlado");

        context.Result = new ObjectResult(new
        {
            error = "Ha ocurrido un error interno en el servidor",
            type = "InternalServerError"
        })
        {
            StatusCode = 500
        };
        context.ExceptionHandled = true;
    }
}