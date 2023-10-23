using Microsoft.EntityFrameworkCore;

namespace WebLoadBalancer.Models
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {
            Database.EnsureCreated();
        }

        public DbSet<web_user> Users { get; set; }
        public DbSet<EquationSol> EquationSols { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<web_user>().ToTable("web_user");
            modelBuilder.Entity<EquationSol>().ToTable("equation");
        }
    }
    
    
}
