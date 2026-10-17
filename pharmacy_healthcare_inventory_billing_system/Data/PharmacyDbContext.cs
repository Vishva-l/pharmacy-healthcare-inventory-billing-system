using Microsoft.EntityFrameworkCore;
using pharmacy_healthcare_inventory_billing_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pharmacy_healthcare_inventory_billing_system.Data
{
    public class PharmacyDbContext : DbContext
    {
        public PharmacyDbContext(
            DbContextOptions<PharmacyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicine> Medicines { get; set; }

        public DbSet<Prescription> Prescriptions { get; set; }

        public DbSet<PrescriptionItem> PrescriptionItems
        {
            get; set;
        }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleItem> SaleItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Medicine>()
                .Property(x => x.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Medicine>()
                .Property(x => x.SellingPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(x => x.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SaleItem>()
                .Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(x => x.Prescription)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(x => x.Medicine)
                .WithMany()
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(x => x.Medicine)
                .WithMany(x => x.SaleItems)
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.Sale)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.SaleId);
        }
    }
}
