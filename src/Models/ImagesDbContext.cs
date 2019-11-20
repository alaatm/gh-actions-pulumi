using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class ImagesDbContext: DbContext
    {
        public DbSet<Image> Images { get; set; }

        public ImagesDbContext(DbContextOptions<ImagesDbContext> options) : base(options) {}
    }
}
