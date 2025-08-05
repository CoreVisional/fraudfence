namespace FraudFence.EntityModels.common
{
    public abstract class BaseEntity
    {
        #region Properties

        public int Id { get; set; }

        public bool IsDisabled { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime LastModified { get; set; }

        #endregion
    }
}
