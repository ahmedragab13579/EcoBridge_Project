using EcoBridge.Domains.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Data;

public class EcoBridgeDbContext(DbContextOptions<EcoBridgeDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
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
            entity.Property(x => x.FullName).HasColumnType("nvarchar(150)").IsRequired();
            entity.Property(x => x.Email).HasColumnType("nvarchar(255)").IsRequired();
            entity.Property(x => x.Phone).HasColumnType("nvarchar(20)").IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.RoleId).HasColumnType("tinyint").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

            entity.HasOne(x => x.Donor)
                .WithOne(x => x.Account)
                .HasForeignKey<Donor>(x => x.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Charity)
                .WithOne(x => x.Account)
                .HasForeignKey<Charity>(x => x.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Volunteer)
                .WithOne(x => x.Account)
                .HasForeignKey<Volunteer>(x => x.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Admin)
                .WithOne(x => x.Account)
                .HasForeignKey<Admin>(x => x.AccountId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Donor>(entity =>
        {
            entity.Property(x => x.UserType).HasColumnType("tinyint").IsRequired();
            entity.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)");
        });

        modelBuilder.Entity<Charity>(entity =>
        {
            entity.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)").IsRequired();
            entity.Property(x => x.RegistrationNumber).HasColumnType("nvarchar(50)").IsRequired();
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.Property(x => x.VehicleDetails).HasColumnType("nvarchar(100)");
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
