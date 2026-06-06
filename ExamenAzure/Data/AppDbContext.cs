using ExamenAzure.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamenAzure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<Movie> Movies { get; set; }
    }
}
