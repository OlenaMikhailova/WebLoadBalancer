using Microsoft.EntityFrameworkCore;


namespace WebLoadBalancer.Models
{
    public class WebDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public WebDbContext(DbContextOptions<WebDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }

}
