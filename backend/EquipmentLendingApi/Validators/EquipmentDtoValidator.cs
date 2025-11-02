using EquipmentLendingApi.Dtos;
using FluentValidation;

namespace EquipmentLendingApi.Validators
{
    public class EquipmentDtoValidator : AbstractValidator<EquipmentDto>
    {
        public EquipmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Equipment name is required")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000");
        }
    }
}
