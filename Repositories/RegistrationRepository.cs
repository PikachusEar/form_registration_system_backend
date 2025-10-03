using Microsoft.EntityFrameworkCore;
using APRegistrationAPI.Data;
using APRegistrationAPI.Models;

namespace APRegistrationAPI.Repositories
{
    public interface IRegistrationRepository
    {
        Task<Registration?> GetByIdAsync(Guid id);
        Task<IEnumerable<Registration>> GetAllAsync();
        Task<Registration> CreateAsync(Registration registration);
        Task<Registration?> UpdateAsync(Registration registration);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task AddAuditAsync(RegistrationAudit audit);
    }

    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegistrationRepository> _logger;

        public RegistrationRepository(ApplicationDbContext context, ILogger<RegistrationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Registration?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.AuditHistory)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registration with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Registration>> GetAllAsync()
        {
            try
            {
                return await _context.Registrations
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all registrations");
                throw;
            }
        }

        public async Task<Registration> CreateAsync(Registration registration)
        {
            try
            {
                registration.Id = Guid.NewGuid();
                registration.CreatedAt = DateTime.UtcNow;
                registration.PaymentStatus = "Pending";

                _context.Registrations.Add(registration);
                
                var audit = new RegistrationAudit
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = registration.Id,
                    Action = "Created",
                    ChangedBy = "System",
                    ChangedAt = DateTime.UtcNow,
                    Notes = "Registration created"
                };
                _context.RegistrationAudits.Add(audit);

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Registration created successfully. ID: {Id}", registration.Id);
                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration");
                throw;
            }
        }

        public async Task<Registration?> UpdateAsync(Registration registration)
        {
            try
            {
                var existing = await _context.Registrations.FindAsync(registration.Id);
                if (existing == null)
                    return null;

                var changes = new List<string>();
                if (existing.PaymentStatus != registration.PaymentStatus)
                    changes.Add($"Payment Status: {existing.PaymentStatus} -> {registration.PaymentStatus}");

                existing.FirstName = registration.FirstName;
                existing.LastName = registration.LastName;
                existing.Email = registration.Email;
                existing.HomePhone = registration.HomePhone;
                existing.MobilePhone = registration.MobilePhone;
                existing.CurrentSchool = registration.CurrentSchool;
                existing.Grade = registration.Grade;
                existing.ExamSection = registration.ExamSection;
                existing.PaymentStatus = registration.PaymentStatus;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = registration.UpdatedBy ?? "System";

                if (changes.Any())
                {
                    var audit = new RegistrationAudit
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = registration.Id,
                        Action = "Updated",
                        ChangedBy = existing.UpdatedBy,
                        ChangedAt = DateTime.UtcNow,
                        Notes = string.Join("; ", changes)
                    };
                    _context.RegistrationAudits.Add(audit);
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Registration updated successfully. ID: {Id}", registration.Id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration with ID: {Id}", registration.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(id);
                if (registration == null)
                    return false;

                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Registration deleted successfully. ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting registration with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Registrations.AnyAsync(r => r.Id == id);
        }

        public async Task AddAuditAsync(RegistrationAudit audit)
        {
            try
            {
                _context.RegistrationAudits.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding audit entry");
                throw;
            }
        }
    }
}