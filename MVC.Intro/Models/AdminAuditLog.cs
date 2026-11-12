namespace MVC.Intro.Models
{
    /// <summary>
    /// Audit trail for important administrator actions.
    /// </summary>
    public class AdminAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Email of the admin who performed the action.</summary>
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>Short action description (e.g. "ProductCreated", "OrderStatusChanged").</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Human-readable description of what was done.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Name of the entity type affected (e.g. "Product", "Order").</summary>
        public string? EntityType { get; set; }

        /// <summary>Primary key of the affected entity.</summary>
        public string? EntityId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
