using EquipmentLendingApi.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace EquipmentLendingApi.Filters
{
    public class CustomValidationResultFactory : IFluentValidationAutoValidationResultFactory
    {
        public IActionResult CreateActionResult(
            ActionExecutingContext context,
            ValidationProblemDetails? validationProblemDetails)
        {
            // Extract error messages from validation problem details
            var errors = validationProblemDetails?.Errors
                .SelectMany(kvp => kvp.Value)
                .ToList() ?? new List<string>();

            // Create your custom ApiResponse format
            var response = ApiResponse<object>.ErrorResponse(
                message: "Validation failed",
                statusCode: 400,
                errors: errors
            );

            return new BadRequestObjectResult(response);
        }
    }
}
