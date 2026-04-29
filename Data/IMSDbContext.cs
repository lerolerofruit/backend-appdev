using IMS_API_.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace IMS_API_.Data;

public class IMSDbContext(DbContextOptions<IMSDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VehiclePart> VehicleParts => Set<VehiclePart>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<CreditPayment> CreditPayments => Set<CreditPayment>();
    public DbSet<ServiceAppointment> ServiceAppointments => Set<ServiceAppointment>();
    public DbSet<CustomerReview> CustomerReviews => Set<CustomerReview>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<FailurePredictionAlert> FailurePredictionAlerts => Set<FailurePredictionAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(150);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalSpend).HasPrecision(18, 2);
            entity.Property(x => x.OutstandingCredit).HasPrecision(18, 2);

            entity.HasOne(x => x.User)
                .WithOne(x => x.CustomerProfile)
                .HasForeignKey<Customer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RegisteredByStaff)
                .WithMany(x => x.RegisteredCustomers)
                .HasForeignKey(x => x.RegisteredByStaffId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.VehicleNumber).IsUnique();
            entity.HasIndex(x => x.Vin).IsUnique();
            entity.Property(x => x.VehicleNumber).HasMaxLength(20);
            entity.Property(x => x.Make).HasMaxLength(60);
            entity.Property(x => x.Model).HasMaxLength(60);
            entity.Property(x => x.Vin).HasMaxLength(50);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.ContactPerson).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.Address).HasMaxLength(250);
        });

        modelBuilder.Entity<VehiclePart>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PartNumber).IsUnique();
            entity.HasIndex(x => x.StockQuantity);
            entity.Property(x => x.PartNumber).HasMaxLength(50);
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(x => x.PrimaryVendor)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.PrimaryVendorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.Property(x => x.InvoiceNumber).HasMaxLength(50);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(x => x.Vendor)
                .WithMany(x => x.PurchaseInvoices)
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CreatedByAdmin)
                .WithMany(x => x.PurchaseInvoices)
                .HasForeignKey(x => x.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);

            entity.HasOne(x => x.PurchaseInvoice)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.VehiclePart)
                .WithMany(x => x.PurchaseInvoiceItems)
                .HasForeignKey(x => x.VehiclePartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.Property(x => x.InvoiceNumber).HasMaxLength(50);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentStatus).HasConversion<string>();
            entity.Property(x => x.EmailSentTo).HasMaxLength(200);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.SalesInvoices)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProcessedByStaff)
                .WithMany(x => x.SalesInvoices)
                .HasForeignKey(x => x.ProcessedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesInvoiceItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);

            entity.HasOne(x => x.SalesInvoice)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.VehiclePart)
                .WithMany(x => x.SalesInvoiceItems)
                .HasForeignKey(x => x.VehiclePartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CreditPayment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AmountPaid).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(300);

            entity.HasOne(x => x.SalesInvoice)
                .WithMany(x => x.CreditPayments)
                .HasForeignKey(x => x.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceAppointment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.AppointmentDate);
            entity.Property(x => x.ServiceType).HasMaxLength(120);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<string>();

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.ServiceAppointments)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Vehicle)
                .WithMany(x => x.ServiceAppointments)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerReview>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Comment).HasMaxLength(1000);
            entity.HasCheckConstraint("CK_CustomerReview_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestedPartName).HasMaxLength(200);
            entity.Property(x => x.RequestedPartNumber).HasMaxLength(80);
            entity.Property(x => x.Status).HasConversion<string>();

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.PartRequests)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FailurePredictionAlert>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.IsResolved);
            entity.Property(x => x.PredictionSummary).HasMaxLength(500);
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 2);

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.PredictionAlerts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Vehicle)
                .WithMany(x => x.PredictionAlerts)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VehiclePart)
                .WithMany(x => x.PredictionAlerts)
                .HasForeignKey(x => x.VehiclePartId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
