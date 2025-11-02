using EquipmentLendingApi.Model;
using Microsoft.EntityFrameworkCore;

namespace EquipmentLendingApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.Property(x => x.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Equipment>(e =>
            {
                e.Property(x => x.Id).ValueGeneratedOnAdd();

                e.HasKey(x => x.Id);

                // One Equipment can have many Requests
                e.HasMany<Request>()
                    .WithOne(r => r.Equipment)
                    .HasForeignKey(r => r.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Request>(r =>
            {
                r.Property(x => x.Id).ValueGeneratedOnAdd();

                r.HasKey(x => x.Id);

                // Request belongs to one User (requester)
                r.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Request belongs to one Equipment
                r.HasOne(x => x.Equipment)
                    .WithMany()
                    .HasForeignKey(x => x.EquipmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Request can be approved by one User (approver)
                r.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }
    }
}
