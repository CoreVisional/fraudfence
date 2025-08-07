namespace FraudFence.EntityModels.Common
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? ModifiedBy { get; set; }
        public bool IsDisabled { get; set; }
    }
}
