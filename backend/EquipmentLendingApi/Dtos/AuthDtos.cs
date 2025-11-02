namespace EquipmentLendingApi.Dtos
{
    public record UserRegisterDto(string FullName, string Email, string Password, string Role);
    public record UserLoginDto(string Email, string Password);
}
