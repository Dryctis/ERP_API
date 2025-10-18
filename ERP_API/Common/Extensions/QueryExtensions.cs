using ERP_API.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Common.Extensions;

public static class QueryExtensions
{
    
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable  
    {
        return query.Where(e => !e.IsDeleted);
    }

    
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable 
    {
        return query.IgnoreQueryFilters();
    }

   
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query)
        where T : class, ISoftDeletable  
    {
        return query.IgnoreQueryFilters().Where(e => e.IsDeleted);
    }

    
    public static void SoftDelete<T>(this T entity)
        where T : class, ISoftDeletable  
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
    }

   
    public static void Restore<T>(this T entity)
        where T : class, ISoftDeletable  
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
    }
}