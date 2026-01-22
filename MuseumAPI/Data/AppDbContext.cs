using Microsoft.EntityFrameworkCore;
using MuseumAPI.Model;

namespace MuseumAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<LoginResponseDto> LoginResponse { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginResponseDto>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
