using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodProject.Data.Models
{
    public class Context: IdentityDbContext<AppUser, AppRole, int>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { optionsBuilder.UseSqlServer("workstation id=FoodOnlinee.mssql.somee.com;packet size=4096;user id=fadel200006_SQLLogin_2;pwd=hvajks96xf;data source=FoodOnlinee.mssql.somee.com;persist security info=False;initial catalog=FoodOnlinee;TrustServerCertificate=True"); }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<About> Abouts { get; set; }        
        public DbSet<Shopping> Shoppings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithMany()
                .HasForeignKey(o => o.PaymentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.AppUser)
                .WithMany()
                .HasForeignKey(o => o.AppUserID)
                .OnDelete(DeleteBehavior.NoAction);
        }

    }
}
