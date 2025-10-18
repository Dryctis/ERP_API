namespace ERP_API.Common.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    
    DateTime? DeletedAt { get; set; }

    
    Guid? DeletedBy { get; set; }
}