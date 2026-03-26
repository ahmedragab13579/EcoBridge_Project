using EcoBridge.Domains.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Data;

public class EcoBridgeDbContext(DbContextOptions<EcoBridgeDbContext> options)
    : IdentityDbContext<Account, IdentityRole<int>, int>(options)
{
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Charity> Charities => Set<Charity>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Donation> Donations => Set<Donation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            // PRD intent: Account "is" the identity user.
            entity.ToTable("Accounts");

            entity.Property(x => x.FullName).HasColumnType("nvarchar(150)").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

            // Keep legacy column name if/when the existing migration is reused.
            entity.Property(x => x.PhoneNumber)
                .HasColumnName("Phone")
                .HasColumnType("nvarchar(20)");

            entity.Property(x => x.Email)
                .HasColumnType("nvarchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(x => x.AccountId);
            entity.Property(x => x.UserType).HasColumnType("tinyint").IsRequired();
            entity.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)");

            entity.HasOne(x => x.Account)
                .WithOne(x => x.Donor)
                .HasForeignKey<Donor>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Charity>(entity =>
        {
            entity.HasKey(x => x.AccountId);
            entity.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)").IsRequired();
            entity.Property(x => x.RegistrationNumber).HasColumnType("nvarchar(50)").IsRequired();

            entity.HasOne(x => x.Account)
                .WithOne(x => x.Charity)
                .HasForeignKey<Charity>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(x => x.AccountId);
            entity.Property(x => x.VehicleDetails).HasColumnType("nvarchar(100)");

            entity.HasOne(x => x.Account)
                .WithOne(x => x.Volunteer)
                .HasForeignKey<Volunteer>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(x => x.AccountId);
            entity.HasOne(x => x.Account)
                .WithOne(x => x.Admin)
                .HasForeignKey<Admin>(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.Property(x => x.FoodType).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(x => x.Quantity).HasColumnType("nvarchar(50)").IsRequired();
            entity.Property(x => x.ExpiryDate).HasColumnType("datetime2").IsRequired();
            entity.Property(x => x.PickupLocation).HasColumnType("nvarchar(255)").IsRequired();
            entity.Property(x => x.ImageUrl).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Status).HasColumnType("tinyint").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

            entity.HasOne(x => x.Donor)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Charity)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.CharityId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Volunteer)
                .WithMany(x => x.Donations)
                .HasForeignKey(x => x.VolunteerId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
