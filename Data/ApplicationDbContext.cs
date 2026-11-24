using GearGrid.Models;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<RentalOrder> RentalOrders => Set<RentalOrder>();
    public DbSet<RentalAgreement> RentalAgreements => Set<RentalAgreement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<RentalOrder>()
            .HasIndex(order => order.OrderReference)
            .IsUnique();

        modelBuilder.Entity<Company>()
            .HasOne(company => company.Owner)
            .WithOne(user => user.Company)
            .HasForeignKey<Company>(company => company.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Company>()
            .HasOne(company => company.VerifiedByAdmin)
            .WithMany()
            .HasForeignKey(company => company.VerifiedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalOrder>()
            .HasOne(order => order.Renter)
            .WithMany(user => user.RentalOrders)
            .HasForeignKey(order => order.RenterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalOrder>()
            .HasOne(order => order.Company)
            .WithMany(company => company.RentalOrders)
            .HasForeignKey(order => order.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalOrder>()
            .HasOne(order => order.Equipment)
            .WithMany(equipment => equipment.RentalOrders)
            .HasForeignKey(order => order.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalOrder>()
            .HasOne(order => order.RentalAgreement)
            .WithOne(agreement => agreement.RentalOrder)
            .HasForeignKey<RentalAgreement>(agreement => agreement.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
