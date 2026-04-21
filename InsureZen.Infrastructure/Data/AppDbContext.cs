using Microsoft.EntityFrameworkCore;
using InsureZen.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace InsureZen.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MakerReview> MakerReviews { get; set; }
        public DbSet<CheckerReview> CheckerReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Claim configuration
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.InsuranceCompany);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.ClaimNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PatientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.InsuranceCompany).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Diagnosis).HasMaxLength(500);
                entity.Property(e => e.Procedure).HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExtractedData).HasColumnType("jsonb");
                entity.Property(e => e.RowVersion).IsRowVersion();
                
                entity.HasOne(e => e.Maker)
                    .WithMany()
                    .HasForeignKey(e => e.MakerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Checker)
                    .WithMany()
                    .HasForeignKey(e => e.CheckerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            });

            // MakerReview configuration
            modelBuilder.Entity<MakerReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimId).IsUnique();
                
                entity.Property(e => e.Feedback).HasMaxLength(2000);
            });

            // CheckerReview configuration
            modelBuilder.Entity<CheckerReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimId).IsUnique();
                
                entity.Property(e => e.Feedback).HasMaxLength(2000);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}