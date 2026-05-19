namespace Goatify.Core.Models;

public abstract class BaseAuditEntity
{
    public string? CreatedBy { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
    public bool DeletedFlag { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}