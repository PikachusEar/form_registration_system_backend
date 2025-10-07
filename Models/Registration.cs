using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APRegistrationAPI.Models
{
    public class Registration
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? HomePhone { get; set; }


        [MaxLength(20)]
        public string MobilePhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CurrentSchool { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ExamSection { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public ICollection<RegistrationAudit> AuditHistory { get; set; } = new List<RegistrationAudit>();
    }

    public class RegistrationAudit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RegistrationId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string ChangedBy { get; set; } = "System";

        [Required]
        public DateTime ChangedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ForeignKey("RegistrationId")]
        public Registration Registration { get; set; } = null!;
    }
}