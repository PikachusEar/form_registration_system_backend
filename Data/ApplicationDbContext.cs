using Microsoft.EntityFrameworkCore;
using APRegistrationAPI.Models;

namespace APRegistrationAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Registration> Registrations { get; set; }
        public DbSet<RegistrationAudit> RegistrationAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.ToTable("Registrations");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.IdempotencyKey)
                    .IsUnique()
                    .HasDatabaseName("IX_Registrations_IdempotencyKey");


                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.PaymentStatus);

                entity.HasMany(e => e.AuditHistory)
                    .WithOne(a => a.Registration)
                    .HasForeignKey(a => a.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RegistrationAudit>(entity =>
            {
                entity.ToTable("RegistrationAudits");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ChangedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.RegistrationId);
                entity.HasIndex(e => e.ChangedAt);
            });
        }

        public override int SaveChanges()
        {
            HandleAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Registration && 
                           (e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var registration = (Registration)entry.Entity;
                registration.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}