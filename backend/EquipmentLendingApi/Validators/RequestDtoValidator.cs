using EquipmentLendingApi.Dtos;
using FluentValidation;

namespace EquipmentLendingApi.Validators
{
    public class RequestDtoValidator : AbstractValidator<RequestDto>
    {
        public RequestDtoValidator()
        {
            RuleFor(x => x.EquipmentId)
                .NotEmpty().WithMessage("Equipment ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity is required.");
        }
    }
}
