namespace GSAuth.Models;

public abstract class _BaseEntity
{
    public virtual long Id { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.Now;
    public virtual DateTime? UpdatedAt { get; set; } 
}
