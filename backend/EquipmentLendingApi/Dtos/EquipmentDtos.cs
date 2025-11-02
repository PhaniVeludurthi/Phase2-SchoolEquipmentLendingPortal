namespace EquipmentLendingApi.Dtos
{
    public record EquipmentDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
    }
}
