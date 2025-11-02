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
    /// Controller for managing equipment inventory
    /// </summary>
    [ApiController, Route("api/equipment")]
    public class EquipmentController(AppDbContext db, ILogger<EquipmentController> logger) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly ILogger<EquipmentController> _logger = logger;

        /// <summary>
        /// Get a list of all available equipment
        /// </summary>
        /// <returns>List of all non-deleted equipment items ordered by name</returns>
        /// <response code="200">Equipment list retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<Equipment>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> List()
        {
            _logger.LogInformation("Fetching equipment list");
            var equipment = await _db.Equipment.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).ToListAsync();
            _logger.LogInformation("Retrieved {Count} equipment items", equipment.Count);
            return Ok(ApiResponse<List<Equipment>>.SuccessResponse(equipment, "Equipment list retrieved successfully"));
        }

        /// <summary>
        /// Get equipment details by ID
        /// </summary>
        /// <param name="id">The unique identifier of the equipment</param>
        /// <returns>Equipment details or error if not found</returns>
        /// <response code="200">Equipment retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="404">Equipment not found or has been deleted</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            _logger.LogInformation("Fetching equipment with ID: {Id}", id);
            var equipment = await _db.Equipment.FindAsync(id);

            if (equipment == null || equipment.IsDeleted)
            {
                _logger.LogWarning("Equipment not found with ID: {Id}", id);
                return NotFound(ApiResponse<Equipment>.NotFoundResponse($"Equipment with ID {id} not found"));
            }

            return Ok(ApiResponse<Equipment>.SuccessResponse(equipment, "Equipment retrieved successfully"));
        }

        /// <summary>
        /// Add new equipment to the inventory
        /// </summary>
        /// <param name="dto">Equipment details including name, category, quantity, condition, and description</param>
        /// <returns>Created equipment object with generated ID</returns>
        /// <response code="200">Equipment added successfully</response>
        /// <response code="400">Bad request. Validation failed or equipment name already exists.</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="403">Forbidden. Admin role required.</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Add(EquipmentDto dto)
        {
            _logger.LogInformation("Adding new equipment: {Name}", dto.Name);

            // Validate input
            if (dto.Quantity < 0 || dto.AvailableQuantity < 0)
            {
                return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                    "Quantity values cannot be negative", 400));
            }

            if (dto.AvailableQuantity > dto.Quantity)
            {
                return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                    "Available quantity cannot exceed total quantity", 400));
            }

            // Check for duplicate name
            if (await _db.Equipment.AnyAsync(e =>
                e.Name.ToLower() == dto.Name.ToLower() && !e.IsDeleted))
            {
                _logger.LogWarning("Equipment with name already exists: {Name}", dto.Name);
                return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                    "Equipment with this name already exists", 400));
            }

            var equipment = new Equipment
            {
                Name = dto.Name,
                Category = dto.Category,
                Condition = dto.Condition,
                AvailableQuantity = dto.AvailableQuantity,
                Description = dto.Description,
                Quantity = dto.Quantity,
            };

            await _db.Equipment.AddAsync(equipment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Equipment added successfully with ID: {Id}", equipment.Id);
            return Ok(ApiResponse<Equipment>.SuccessResponse(equipment, "Equipment added successfully"));
        }

        /// <summary>
        /// Update existing equipment details
        /// </summary>
        /// <param name="id">The unique identifier of the equipment to update</param>
        /// <param name="dto">Updated equipment details</param>
        /// <returns>Updated equipment object</returns>
        /// <response code="200">Equipment updated successfully</response>
        /// <response code="400">Bad request. Validation failed, quantity constraints violated, or equipment name already exists.</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="403">Forbidden. Admin role required.</response>
        /// <response code="404">Equipment not found or has been deleted</response>
        /// <response code="409">Conflict. Equipment was updated by another user (concurrency conflict).</response>
        /// <response code="500">Internal server error occurred during update.</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Equipment>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, EquipmentDto dto)
        {
            _logger.LogInformation("Updating equipment with ID: {Id}", id);
            // Use transaction to ensure consistency
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Lock the equipment row to prevent concurrent updates
                var equipment = await _db.Equipment
                           .FromSqlRaw(@"
                SELECT * FROM ""Equipment"" 
                WHERE ""Id"" = {0} 
                FOR UPDATE", id)
                           .FirstOrDefaultAsync();

                if (equipment == null || equipment.IsDeleted)
                {
                    _logger.LogWarning("Equipment not found with ID: {Id}", id);
                    return NotFound(ApiResponse<Equipment>.NotFoundResponse(
                        $"Equipment with ID {id} not found"));
                }

                // Validate input
                if (dto.Quantity < 0 || dto.AvailableQuantity < 0)
                {
                    return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                        "Quantity values cannot be negative", 400));
                }

                // Check for duplicate name (excluding current equipment)
                if (await _db.Equipment.AnyAsync(e =>
                    e.Name.ToLower() == dto.Name.ToLower() &&
                    e.Id != id &&
                    !e.IsDeleted))
                {
                    _logger.LogWarning("Equipment with name already exists: {Name}", dto.Name);
                    return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                        "Equipment with this name already exists", 400));
                }

                // Calculate reserved quantity (approved or issued requests)
                var reservedQuantity = await _db.Requests
                    .Where(r => r.EquipmentId == id &&
                               (r.Status.ToLower() == "approved" || r.Status.ToLower() == "issued"))
                    .SumAsync(r => r.Quantity);

                _logger.LogInformation(
                    "Equipment {Id} has {Reserved} units reserved out of {Total}",
                    id, reservedQuantity, equipment.Quantity);

                // If updating total quantity, ensure it's not less than reserved quantity
                if (dto.Quantity < reservedQuantity)
                {
                    _logger.LogWarning(
                        "Cannot reduce quantity below reserved amount. Reserved: {Reserved}, New Total: {NewTotal}",
                        reservedQuantity, dto.Quantity);
                    return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                        $"Cannot set total quantity to {dto.Quantity}. " +
                        $"There are {reservedQuantity} units currently reserved in active requests. " +
                        $"Please complete or cancel those requests first.", 400));
                }

                // Calculate the change in total quantity
                var quantityChange = dto.Quantity - equipment.Quantity;

                // Update equipment properties
                equipment.Name = dto.Name;
                equipment.Category = dto.Category;
                equipment.Quantity = dto.Quantity;
                equipment.Description = dto.Description;
                equipment.Condition = dto.Condition;

                var newAvailableQuantity = equipment.AvailableQuantity + quantityChange;

                // Ensure available quantity doesn't go negative
                if (newAvailableQuantity < 0)
                {
                    return BadRequest(ApiResponse<Equipment>.ErrorResponse(
                        $"Cannot reduce total quantity by {Math.Abs(quantityChange)} units. " +
                        $"This would result in negative available quantity. " +
                        $"Current available: {equipment.AvailableQuantity}", 400));
                }

                // Ensure available quantity doesn't exceed total quantity
                if (newAvailableQuantity > dto.Quantity)
                {
                    newAvailableQuantity = dto.Quantity;
                }

                equipment.AvailableQuantity = newAvailableQuantity;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Equipment updated successfully: {Id}. Total: {Total}, Available: {Available}, Reserved: {Reserved}",
                    id, equipment.Quantity, equipment.AvailableQuantity, reservedQuantity);

                return Ok(ApiResponse<Equipment>.SuccessResponse(
                    equipment, "Equipment updated successfully"));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Concurrency conflict updating equipment: {Id}", id);
                return Conflict(ApiResponse<Equipment>.ErrorResponse(
                    "Equipment was updated by another user. Please refresh and try again.", 409));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating equipment: {Id}", id);
                return StatusCode(500, ApiResponse<Equipment>.ErrorResponse(
                    "An error occurred while updating equipment", 500));
            }
        }

        /// <summary>
        /// Soft delete equipment from the inventory
        /// </summary>
        /// <param name="id">The unique identifier of the equipment to delete</param>
        /// <returns>Success message confirming deletion</returns>
        /// <response code="200">Equipment deleted successfully</response>
        /// <response code="400">Bad request. Equipment ID is empty, already deleted, or has active requests.</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="403">Forbidden. Admin role required.</response>
        /// <response code="404">Equipment not found</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Attempting to delete equipment with ID: {Id}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Delete request failed: Empty equipment ID provided.");
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Equipment ID is required.", 400));
            }

            var equipment = await _db.Equipment.FindAsync(id);

            if (equipment == null)
            {
                _logger.LogWarning("Delete request failed: Equipment not found with ID: {Id}", id);
                return NotFound(ApiResponse<object>.NotFoundResponse(
                    $"Equipment with ID '{id}' not found."));
            }

            // Check if already deleted
            if (equipment.IsDeleted)
            {
                _logger.LogWarning("Delete request ignored: Equipment {Id} already marked as deleted.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "This equipment record is already deleted.", 400));
            }

            // Validation: Check for active or pending requests
            var hasActiveRequests = await _db.Requests.AnyAsync(r =>
                r.EquipmentId == id &&
                (r.Status.ToLower() == "pending" ||
                 r.Status.ToLower() == "approved" ||
                 r.Status.ToLower() == "issued"));

            if (hasActiveRequests)
            {
                _logger.LogWarning("Delete request blocked: Equipment {Id} has active requests.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot delete equipment that has active, pending, or issued requests. " +
                    "Please complete, cancel, or return them first.", 400));
            }

            // Perform soft delete
            equipment.IsDeleted = true;
            equipment.DeletedAt = DateTime.UtcNow;
            equipment.DeletedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                  User.Identity?.Name ??
                                  "System";

            _db.Equipment.Update(equipment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Equipment deleted successfully: {Id}", id);
            return Ok(ApiResponse<object>.SuccessResponse(
                null, $"Equipment '{equipment.Name}' deleted successfully."));
        }

        /// <summary>
        /// Get detailed availability information for specific equipment
        /// </summary>
        /// <param name="id">The unique identifier of the equipment</param>
        /// <returns>Availability details including total quantity, available quantity, reserved quantity, and availability status</returns>
        /// <response code="200">Availability information retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="404">Equipment not found or has been deleted</response>
        [HttpGet("{id}/availability")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailability(string id)
        {
            _logger.LogInformation("Fetching availability for equipment: {Id}", id);

            var equipment = await _db.Equipment.FindAsync(id);

            if (equipment == null || equipment.IsDeleted)
            {
                return NotFound(ApiResponse<object>.NotFoundResponse(
                    "Equipment not found"));
            }

            // Calculate reserved quantity
            var reservedQuantity = await _db.Requests
                .Where(r => r.EquipmentId == id &&
                           (r.Status.ToLower() == "approved" || r.Status.ToLower() == "issued"))
                .SumAsync(r => r.Quantity);

            var availabilityInfo = new
            {
                equipment.Id,
                equipment.Name,
                TotalQuantity = equipment.Quantity,
                AvailableQuantity = equipment.AvailableQuantity,
                ReservedQuantity = reservedQuantity,
                IsAvailable = equipment.AvailableQuantity > 0
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                availabilityInfo, "Availability information retrieved"));
        }

    }
}
