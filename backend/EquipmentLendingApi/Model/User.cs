namespace EquipmentLendingApi.Model
{
    public class User
    {
        public string Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = ""; // Student, Staff, Admin
        public ICollection<Request> Requests { get; set; } = [];
        public ICollection<Request> ApprovedRequests { get; set; } = [];
    }
}
