namespace EquipmentLendingApi.Dtos
{
    public record RequestDto(string EquipmentId, int Quantity, string Notes);
    public record ApproveDto(bool Approve);
    public class UpdateRequestDto
    {
        public string? Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class RequestResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string EquipmentId { get; set; }
        public int Quantity { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public UserDto? User { get; set; }
        public UserDto? Approver { get; set; }
        public EquipmentResponseDto? Equipment { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        // Add other user properties as needed
    }

    public class EquipmentResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Description { get; set; }
        public string? Condition { get; set; }
    }
}
