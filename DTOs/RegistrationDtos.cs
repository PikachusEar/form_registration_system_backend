using System.ComponentModel.DataAnnotations;

namespace APRegistrationAPI.DTOs
{
    public class CreateRegistrationDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? HomePhone { get; set; }

        [Required(ErrorMessage = "Mobile phone is required")]
        [MaxLength(20)]
        public string MobilePhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current school is required")]
        [MaxLength(200)]
        public string CurrentSchool { get; set; } = string.Empty;

        [Required(ErrorMessage = "Grade is required")]
        [MaxLength(10)]
        public string Grade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Exam section is required")]
        [MaxLength(100)]
        public string ExamSection { get; set; } = string.Empty;
    }

    public class RegistrationResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HomePhone { get; set; }
        public string MobilePhone { get; set; } = string.Empty;
        public string CurrentSchool { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string ExamSection { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class RegistrationDetailDto : RegistrationResponseDto
    {
        public List<AuditHistoryDto> AuditHistory { get; set; } = new();
    }

    public class AuditHistoryDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Notes { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}