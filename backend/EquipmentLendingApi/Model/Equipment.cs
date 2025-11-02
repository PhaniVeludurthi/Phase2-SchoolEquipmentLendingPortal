using System.ComponentModel.DataAnnotations;

namespace EquipmentLendingApi.Model
{
    public class Equipment
    {
        public string Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Description { get; set; }
        public string? Condition { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public ICollection<Request> Requests { get; set; } = [];
    }
}
