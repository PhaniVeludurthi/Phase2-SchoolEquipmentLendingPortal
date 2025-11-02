namespace EquipmentLendingApi.Model
{
    public class Request
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string EquipmentId { get; set; }
        public int Quantity { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime? RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedBy { get; set; }
        public User? User { get; set; }
        public User? Approver { get; set; }
        public Equipment? Equipment { get; set; }
    }
}
