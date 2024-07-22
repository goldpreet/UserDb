using Microsoft.EntityFrameworkCore;

namespace crud_dotnet_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        //public DbSet<User> Users { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<User>().HasKey(u => u.Id); // Configure primary key
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
