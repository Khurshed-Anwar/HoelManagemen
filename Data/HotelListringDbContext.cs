using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data
{
    public class HotelListringDbContext : DbContext
    {
        public HotelListringDbContext(DbContextOptions<HotelListringDbContext> options) : base(options)
        {
        }

        public  DbSet<Hotel> Hotels { get; set; }
        public DbSet<Country> Countries { get; set; }
    }
}
