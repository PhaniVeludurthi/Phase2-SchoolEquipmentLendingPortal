using EquipmentLendingApi.Data;
using EquipmentLendingApi.Dtos;
using EquipmentLendingApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipmentLendingApi.Controllers
{
    /// <summary>
    /// Controller for managing user profile information
    /// </summary>
    [ApiController, Route("api/profile")]
    [Authorize]
    public class ProfileController(AppDbContext db, ILogger<ProfileController> logger) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly ILogger<ProfileController> _logger = logger;

        /// <summary>
        /// Get the current authenticated user's profile information
        /// </summary>
        /// <returns>Current user's profile details including ID, email, full name, and role</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required or user ID not found in token.</response>
        /// <response code="404">User profile not found</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var userProfile = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userProfile == null)
            {
                return NotFound(ApiResponse<ProfileDto>.NotFoundResponse("User not found"));
            }

            return Ok(ApiResponse<ProfileDto>.SuccessResponse(new ProfileDto
            {
                Id = userProfile.Id,
                Email = userProfile.Email,
                FullName = userProfile.FullName,
                Role = userProfile.Role,
            }));
        }
    }
}
