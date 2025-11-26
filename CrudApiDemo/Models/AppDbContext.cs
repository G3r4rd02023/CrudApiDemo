using Microsoft.EntityFrameworkCore;

namespace CrudApiDemo.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
       : base(options)
        {
        }

        public DbSet<Producto> Productos => Set<Producto>();
    }
}
