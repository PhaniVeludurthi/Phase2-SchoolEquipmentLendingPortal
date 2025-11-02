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
    /// Controller for managing equipment lending requests
    /// </summary>
    [ApiController, Route("api/requests")]
    [Authorize]
    public class RequestsController(AppDbContext db, ILogger<RequestsController> logger) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly ILogger<RequestsController> _logger = logger;

        /// <summary>
        /// Get a list of equipment lending requests
        /// </summary>
        /// <param name="status">Optional filter by request status (pending, approved, rejected, issued, returned, cancelled, overdue)</param>
        /// <returns>List of requests. Regular users see only their own requests. Admins and staff see all requests.</returns>
        /// <response code="200">Requests retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Request>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> List([FromQuery] string? status = null)
        {
            var userEmail = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Fetching requests for user: {Email}, Role: {Role}", userEmail, userRole);

            IQueryable<Request> query = _db.Requests
            .Include(r => r.Equipment)
            .Include(r => r.User)
            .Include(r => r.Approver);

            // Filter by role
            if (userRole != "admin" && userRole != "staff")
            {
                query = query.Where(r => r.UserId == userId);
            }

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status.ToLower() == status.ToLower());
            }

            var requests = await query.OrderByDescending(x => x.RequestedAt).ToListAsync();

            _logger.LogInformation("Retrieved {Count} requests", requests.Count);
            return Ok(ApiResponse<List<Request>>.SuccessResponse(requests, "Requests retrieved successfully"));
        }

        /// <summary>
        /// Create a new equipment borrowing request
        /// </summary>
        /// <param name="dto">Request details including equipment ID, quantity, and optional notes</param>
        /// <returns>Created request object with status 'pending'</returns>
        /// <response code="200">Borrow request submitted successfully</response>
        /// <response code="400">Bad request. Validation failed, quantity invalid, equipment unavailable, or user already has pending/active request for this equipment.</response>
        /// <response code="401">Unauthorized. Valid JWT token required or user not found.</response>
        /// <response code="404">Equipment not found or has been deleted</response>
        /// <response code="409">Conflict. Equipment is being updated by another user (concurrency conflict).</response>
        /// <response code="500">Internal server error occurred while processing the request.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Borrow(RequestDto dto)
        {
            var userEmail = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(ApiResponse<Request>.NotFoundResponse("User not found"));
            }
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogInformation("User {User} not found", userId);
                return Unauthorized(ApiResponse<Request>.UnauthorizedResponse("User not found"));
            }
            _logger.LogInformation("Borrow request for equipment {EquipmentId} by user {UserId}", dto.EquipmentId, userId);

            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Check if equipment exists and lock the row
                var equipment = await _db.Equipment
                    .FromSqlRaw(@"
                    SELECT * FROM ""Equipment"" 
                    WHERE ""Id"" = {0} 
                    FOR UPDATE", dto.EquipmentId)
                    .FirstOrDefaultAsync();

                if (equipment == null || equipment.IsDeleted)
                {
                    _logger.LogWarning("Equipment not found: {EquipmentId}", dto.EquipmentId);
                    return NotFound(ApiResponse<Request>.NotFoundResponse("Equipment not found"));
                }

                // Check quantity availability
                if (dto.Quantity <= 0)
                {
                    return BadRequest(ApiResponse<Request>.ErrorResponse("Quantity must be greater than 0", 400));
                }

                if (dto.Quantity > equipment.Quantity)
                {
                    return BadRequest(ApiResponse<Request>.ErrorResponse($"Requested quantity exceeds total available ({equipment.Quantity})", 400));
                }

                var hasPendingRequest = await _db.Requests.AnyAsync(r =>
                                r.UserId == userId &&
                                r.EquipmentId == dto.EquipmentId &&
                                r.Status.ToLower() == "pending");

                if (hasPendingRequest)
                {
                    _logger.LogWarning("User already has pending request for equipment: {EquipmentId}", dto.EquipmentId);
                    return BadRequest(ApiResponse<Request>.ErrorResponse("You already have a pending request for this equipment", 400));
                }

                // Check if user already has approved/issued request for same equipment
                var hasActiveRequest = await _db.Requests.AnyAsync(r =>
                    r.UserId == userId &&
                    r.EquipmentId == dto.EquipmentId &&
                    (r.Status.ToLower() == "approved" || r.Status.ToLower() == "issued"));

                if (hasActiveRequest)
                {
                    _logger.LogWarning("User already has active request for equipment: {EquipmentId}", dto.EquipmentId);
                    return BadRequest(ApiResponse<Request>.ErrorResponse("You already have an active request for this equipment", 400));
                }

                var request = new Request
                {
                    UserId = userId,
                    EquipmentId = dto.EquipmentId,
                    RequestedAt = DateTime.UtcNow,
                    Quantity = dto.Quantity,
                    Notes = dto.Notes,
                    Status = "pending"
                };

                await _db.Requests.AddAsync(request);
                await _db.SaveChangesAsync();

                // Load navigation properties
                await transaction.CommitAsync();

                // Load navigation properties for response
                request.Equipment = equipment;
                request.User = new User { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role };

                _logger.LogInformation("Borrow request created successfully: {RequestId}", request.Id);
                return Ok(ApiResponse<Request>.SuccessResponse(request, "Borrow request submitted successfully"));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Concurrency conflict when creating request for equipment: {EquipmentId}", dto.EquipmentId);
                return Conflict(ApiResponse<Request>.ErrorResponse("Equipment is being updated. Please try again.", 409));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating borrow request");
                return StatusCode(500, ApiResponse<Request>.ErrorResponse("An error occurred while processing your request", 500));
            }
        }

        /// <summary>
        /// Get all pending equipment lending requests
        /// </summary>
        /// <returns>List of all requests with status 'pending', ordered by request date</returns>
        /// <response code="200">Pending requests retrieved successfully</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="403">Forbidden. Staff or Admin role required.</response>
        [HttpGet("pending")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<List<Request>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Pending()
        {
            _logger.LogInformation("Fetching pending requests");

            var pending = await _db.Requests
                .Where(r => r.Status.ToLowerInvariant() == "pending")
                .Include(r => r.Equipment)
                .Include(r => r.User)
                .OrderBy(r => r.RequestedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} pending requests", pending.Count);
            return Ok(ApiResponse<List<Request>>.SuccessResponse(pending, "Pending requests retrieved successfully"));
        }

        /// <summary>
        /// Update an equipment lending request status and details
        /// </summary>
        /// <param name="id">The unique identifier of the request to update</param>
        /// <param name="update">Update details including status, dates, and admin notes</param>
        /// <returns>Updated request object with navigation properties</returns>
        /// <remarks>
        /// Valid status transitions:
        /// - pending → approved, rejected, cancelled
        /// - approved → issued, cancelled
        /// - issued → returned, overdue
        /// - overdue → returned
        /// 
        /// When status changes to 'approved', available quantity is reserved.
        /// When status changes to 'returned' or 'cancelled', reserved quantity is released back.
        /// </remarks>
        /// <response code="200">Request updated successfully</response>
        /// <response code="400">Bad request. Invalid status transition or quantity constraints violated.</response>
        /// <response code="401">Unauthorized. Valid JWT token required.</response>
        /// <response code="403">Forbidden. Staff or Admin role required.</response>
        /// <response code="404">Request or associated equipment not found</response>
        /// <response code="409">Conflict. Request or equipment was updated by another user (concurrency conflict).</response>
        /// <response code="500">Internal server error occurred while updating the request.</response>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<RequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Request>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestDto>> UpdateRequest(string id, [FromBody] UpdateRequestDto update)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Use transaction with appropriate isolation level
            await using var transaction = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted);
            try
            {
                var request = await _db.Requests
                    .Include(r => r.Equipment)
                    .Include(r => r.User)
                    .Include(r => r.Approver)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (request == null)
                {
                    return NotFound(ApiResponse<Request>.NotFoundResponse("Request not found"));
                }
                var currentStatus = request.Status.ToLowerInvariant();
                Equipment? equipment = null;

                // Handle status changes
                if (update.Status != null)
                {
                    var newStatus = update.Status.ToLowerInvariant();

                    // Validate status transition
                    if (!IsValidStatusTransition(currentStatus, newStatus))
                    {
                        return BadRequest(ApiResponse<Request>.ErrorResponse(
                            $"Invalid status transition from {request.Status} to {update.Status}", 400));
                    }

                    // Lock equipment row if we're going to modify quantity
                    if (RequiresEquipmentUpdate(currentStatus, newStatus))
                    {
                        // Use pessimistic locking to prevent race conditions
                        equipment = await _db.Equipment
                                                .FromSqlRaw(@"
                            SELECT * FROM ""Equipment"" 
                            WHERE ""Id"" = {0} 
                            FOR UPDATE", request.EquipmentId)
                                                .FirstOrDefaultAsync();

                        if (equipment == null || equipment.IsDeleted)
                        {
                            return NotFound(ApiResponse<Request>.NotFoundResponse("Equipment not found"));
                        }
                    }

                    // Handle quantity changes based on status transition
                    var quantityChange = CalculateQuantityChange(currentStatus, newStatus, request.Quantity);

                    if (quantityChange != 0 && equipment != null)
                    {
                        var newAvailableQuantity = equipment.AvailableQuantity + quantityChange;

                        // Validate quantity bounds
                        if (newAvailableQuantity < 0)
                        {
                            return BadRequest(ApiResponse<Request>.ErrorResponse(
                                $"Insufficient equipment quantity available. Available: {equipment.AvailableQuantity}, " +
                                $"Requested: {request.Quantity}", 400));
                        }

                        if (newAvailableQuantity > equipment.Quantity)
                        {
                            return BadRequest(ApiResponse<Request>.ErrorResponse(
                                "Available quantity cannot exceed total quantity", 400));
                        }

                        equipment.AvailableQuantity = newAvailableQuantity;
                        _logger.LogInformation(
                            "Equipment {EquipmentId} quantity changed by {Change}. New available: {Available}",
                            equipment.Id, quantityChange, equipment.AvailableQuantity);
                    }

                    // Update request status
                    var oldStatus = request.Status;
                    request.Status = update.Status;

                    // Set status-specific fields
                    switch (newStatus)
                    {
                        case "approved":
                            request.ApprovedAt = update.ApprovedAt ?? DateTime.UtcNow;
                            request.ApprovedBy = currentUserId;
                            request.DueDate = update.DueDate;
                            break;

                        case "rejected":
                            request.RejectedAt = DateTime.UtcNow;
                            request.RejectedBy = currentUserId;
                            break;

                        case "issued":
                            request.IssuedAt = update.IssuedAt ?? DateTime.UtcNow;
                            break;

                        case "returned":
                            request.ReturnedAt = update.ReturnedAt ?? DateTime.UtcNow;
                            break;
                    }

                    _logger.LogInformation(
                        "Request {RequestId} status changed from {OldStatus} to {NewStatus} by {UserId}",
                        id, oldStatus, newStatus, currentUserId);
                }

                // Update other fields if provided
                if (update.ApprovedAt.HasValue && request.ApprovedAt == null)
                {
                    request.ApprovedAt = update.ApprovedAt;
                }

                if (update.ApprovedBy != null && request.ApprovedBy == null)
                {
                    request.ApprovedBy = update.ApprovedBy;
                }

                if (update.IssuedAt.HasValue && request.IssuedAt == null)
                {
                    request.IssuedAt = update.IssuedAt;
                }

                if (update.DueDate.HasValue)
                {
                    request.DueDate = update.DueDate;
                }

                if (update.ReturnedAt.HasValue && request.ReturnedAt == null)
                {
                    request.ReturnedAt = update.ReturnedAt;
                }

                if (update.AdminNotes != null)
                {
                    request.AdminNotes = update.AdminNotes;
                }

                // Save changes and commit transaction
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload navigation properties for response
                await _db.Entry(request).ReloadAsync();
                await _db.Entry(request).Reference(r => r.Equipment).LoadAsync();
                await _db.Entry(request).Reference(r => r.User).LoadAsync();
                await _db.Entry(request).Reference(r => r.Approver).LoadAsync();

                _logger.LogInformation("Request {RequestId} updated successfully", id);
                return Ok(ApiResponse<RequestResponseDto>.SuccessResponse(
                    MapToDto(request), "Request updated successfully"));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Concurrency conflict updating request {RequestId}", id);
                return Conflict(ApiResponse<Request>.ErrorResponse(
                    "Request or equipment was updated by another user. Please refresh and try again.", 409));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error updating request {RequestId}", id);
                return StatusCode(500, ApiResponse<Request>.ErrorResponse(
                    "Error updating request: " + ex.Message, 500));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error updating request {RequestId}", id);
                return StatusCode(500, ApiResponse<Request>.ErrorResponse(
                    "An unexpected error occurred while updating the request", 500));
            }
        }
        private static bool RequiresEquipmentUpdate(string currentStatus, string newStatus)
        {
            // Reserve quantity: pending → approved
            if (currentStatus == "pending" && newStatus == "approved")
                return true;

            // Return quantity: approved/issued → returned/cancelled
            if ((currentStatus == "approved" || currentStatus == "issued") &&
                (newStatus == "returned" || newStatus == "cancelled"))
                return true;

            return false;
        }

        private static int CalculateQuantityChange(string currentStatus, string newStatus, int requestQuantity)
        {
            // Reserve quantity (decrease available): pending → approved
            if (currentStatus == "pending" && newStatus == "approved")
                return -requestQuantity;

            // Return quantity (increase available): approved/issued → returned/cancelled
            if ((currentStatus == "approved" || currentStatus == "issued") &&
                (newStatus == "returned" || newStatus == "cancelled"))
                return requestQuantity;

            return 0;
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<string, List<string>>
        {
            { "pending", new List<string> { "approved", "rejected", "cancelled" } },
            { "approved", new List<string> { "issued", "cancelled" } },
            { "issued", new List<string> { "returned", "overdue" } },
            { "overdue", new List<string> { "returned" } },
            { "returned", new List<string>() },
            { "rejected", new List<string>() },
            { "cancelled", new List<string>() }
        };

            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
        }

        private RequestResponseDto MapToDto(Request request)
        {
            return new RequestResponseDto
            {
                Id = request.Id,
                UserId = request.UserId,
                EquipmentId = request.EquipmentId,
                Quantity = request.Quantity,
                IssuedAt = request.IssuedAt,
                DueDate = request.DueDate,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                ApprovedAt = request.ApprovedAt,
                ApprovedBy = request.ApprovedBy,
                ReturnedAt = request.ReturnedAt,
                Notes = request.Notes,
                AdminNotes = request.AdminNotes,
                User = request.User != null ? new UserDto
                {
                    Id = request.User.Id,
                    // Map other user properties
                } : null,
                Approver = request.Approver != null ? new UserDto
                {
                    Id = request.Approver.Id,
                    // Map other user properties
                } : null,
                Equipment = request.Equipment != null ? new EquipmentResponseDto
                {
                    Id = request.Equipment.Id,
                    Name = request.Equipment.Name,
                    Category = request.Equipment.Category,
                    Quantity = request.Equipment.Quantity,
                    AvailableQuantity = request.Equipment.AvailableQuantity,
                    Description = request.Equipment.Description,
                    Condition = request.Equipment.Condition
                } : null
            };
        }
    }
}
