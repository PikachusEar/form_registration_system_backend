using Microsoft.AspNetCore.Mvc;
using APRegistrationAPI.DTOs;
using APRegistrationAPI.Models;
using APRegistrationAPI.Repositories;
using APRegistrationAPI.Services;

namespace APRegistrationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly IRegistrationRepository _repository;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegistrationsController> _logger;

        public RegistrationsController(
            IRegistrationRepository repository,
            IEmailService emailService,
            ILogger<RegistrationsController> logger)
        {
            _repository = repository;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new AP Exam registration
        /// </summary>
        /// <param name="dto">Registration details</param>
        /// <returns>Created registration with confirmation</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RegistrationResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRegistration([FromBody] CreateRegistrationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Validation failed", errors));
                }

                // Map DTO to entity
                var registration = new Registration
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    HomePhone = dto.HomePhone,
                    MobilePhone = dto.MobilePhone,
                    CurrentSchool = dto.CurrentSchool,
                    Grade = dto.Grade,
                    ExamSection = dto.ExamSection
                };

                // Save to database
                var created = await _repository.CreateAsync(registration);

                // Send confirmation email to student (don't block on email failure)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendConfirmationEmailAsync(
                            created.Email,
                            created.FirstName,
                            created.LastName,
                            created.ExamSection,
                            created.Grade,
                            created.Id.ToString()
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send confirmation email for registration {Id}", created.Id);
                    }
                });

                // Send notification email to staff (don't block on email failure)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendStaffNotificationAsync(
                            created.FirstName,
                            created.LastName,
                            created.Email,
                            created.ExamSection,
                            created.Grade,
                            created.Id.ToString()
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send staff notification for registration {Id}", created.Id);
                    }
                });

                // Map to response DTO
                var responseDto = MapToResponseDto(created);

                return CreatedAtAction(
                    nameof(GetRegistrationById),
                    new { id = created.Id },
                    ApiResponse<RegistrationResponseDto>.SuccessResponse(
                        responseDto,
                        "Registration submitted successfully. Confirmation email has been sent.")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while processing your registration. Please try again."));
            }
        }

        /// <summary>
        /// Get all registrations
        /// </summary>
        /// <returns>List of all registrations</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RegistrationResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRegistrations()
        {
            try
            {
                var registrations = await _repository.GetAllAsync();
                var responseDtos = registrations.Select(MapToResponseDto).ToList();

                return Ok(ApiResponse<IEnumerable<RegistrationResponseDto>>.SuccessResponse(
                    responseDtos,
                    $"Retrieved {responseDtos.Count} registration(s)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registrations");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving registrations"));
            }
        }

        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        /// <param name="id">Registration ID</param>
        /// <returns>Registration details with audit history</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RegistrationDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRegistrationById(Guid id)
        {
            try
            {
                var registration = await _repository.GetByIdAsync(id);

                if (registration == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(
                        $"Registration with ID {id} not found"));
                }

                var detailDto = MapToDetailDto(registration);

                return Ok(ApiResponse<RegistrationDetailDto>.SuccessResponse(
                    detailDto,
                    "Registration retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration {Id}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving the registration"));
            }
        }

        // Helper mapping methods
        private RegistrationResponseDto MapToResponseDto(Registration registration)
        {
            return new RegistrationResponseDto
            {
                Id = registration.Id,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Email = registration.Email,
                HomePhone = registration.HomePhone,
                MobilePhone = registration.MobilePhone,
                CurrentSchool = registration.CurrentSchool,
                Grade = registration.Grade,
                ExamSection = registration.ExamSection,
                PaymentStatus = registration.PaymentStatus,
                CreatedAt = registration.CreatedAt,
                UpdatedAt = registration.UpdatedAt,
                UpdatedBy = registration.UpdatedBy
            };
        }

        private RegistrationDetailDto MapToDetailDto(Registration registration)
        {
            return new RegistrationDetailDto
            {
                Id = registration.Id,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Email = registration.Email,
                HomePhone = registration.HomePhone,
                MobilePhone = registration.MobilePhone,
                CurrentSchool = registration.CurrentSchool,
                Grade = registration.Grade,
                ExamSection = registration.ExamSection,
                PaymentStatus = registration.PaymentStatus,
                CreatedAt = registration.CreatedAt,
                UpdatedAt = registration.UpdatedAt,
                UpdatedBy = registration.UpdatedBy,
                AuditHistory = registration.AuditHistory
                    .OrderByDescending(a => a.ChangedAt)
                    .Select(a => new AuditHistoryDto
                    {
                        Id = a.Id,
                        Action = a.Action,
                        ChangedBy = a.ChangedBy,
                        ChangedAt = a.ChangedAt,
                        OldValue = a.OldValue,
                        NewValue = a.NewValue,
                        Notes = a.Notes
                    })
                    .ToList()
            };
        }
    }
}