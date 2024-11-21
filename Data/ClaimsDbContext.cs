using Claimed.Models;
using Microsoft.EntityFrameworkCore;

public class ClaimsDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Invoice> Invoices { get; set; }

    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>()
        .HasKey(c => c.ClaimId);

        modelBuilder.Entity<Claim>()
            .HasOne(c => c.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(c => c.UserId);

        base.OnModelCreating(modelBuilder);

        // Seed Users (2 normal users, 2 admins)
        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, Username = "user1", PasswordHash = "user1password", IsAdmin = false },
            new User { UserId = 2, Username = "user2", PasswordHash = "user2password", IsAdmin = false },
            new User { UserId = 3, Username = "admin1", PasswordHash = "admin1password", IsAdmin = true },
            new User { UserId = 4, Username = "admin2", PasswordHash = "admin2password", IsAdmin = true }
        );

        // Seed Claims (3 new claims, two with supporting documents and one without)
        modelBuilder.Entity<Claim>().HasData(
            // Existing claim (from previous seed)
            new Claim
            {
                ClaimId = 1,
                UserId = 1,  // Associated with user1
                Course = "Programming 1A",
                Description = "Claim for completing all of Prog 1A before mid-term break",
                HourlyRate = 500.00M,
                HoursWorked = 10.5,
                SupportingDocument = null, // No document attached
                Status = "Pending",
                DateSubmitted = DateTime.Now
            },
            // New Claim 1: With Proof1.txt as a supporting document
            new Claim
            {
                ClaimId = 2,
                UserId = 2,  // Associated with user2
                Course = "Java Programming",
                Description = "Claim for completing Java Programming course",
                HourlyRate = 600.00M,
                HoursWorked = 12.0,
                SupportingDocument = "/uploads/Proof1.txt",  // Document provided
                Status = "Pending",
                DateSubmitted = DateTime.Now
            },
            // New Claim 2: With Proof2.txt as a supporting document
            new Claim
            {
                ClaimId = 3,
                UserId = 1,  // Associated with user1
                Course = "Web Development",
                Description = "Claim for completing Web Development course",
                HourlyRate = 550.00M,
                HoursWorked = 15.0,
                SupportingDocument = "/uploads/Proof2.txt",  // Document provided
                Status = "Pending",
                DateSubmitted = DateTime.Now
            },
            // New Claim 3: Without a supporting document
            new Claim
            {
                ClaimId = 4,
                UserId = 2,  // Associated with user2
                Course = "Advanced Algorithms",
                Description = "Claim for completing Advanced Algorithms",
                HourlyRate = 700.00M,
                HoursWorked = 8.0,
                SupportingDocument = null,  // No document attached
                Status = "Pending",
                DateSubmitted = DateTime.Now
            }
        );

        // Set default values for Claim Status
        modelBuilder.Entity<Claim>()
            .Property(c => c.Status)
            .HasDefaultValue("Pending");
    }

}
