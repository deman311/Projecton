using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Models
{
    /// <summary>
    /// This is the DB handler used by the API, contains just an ImageObj set.
    /// </summary>
    public class ImageContext : DbContext
    {
        public ImageContext(DbContextOptions<ImageContext> options) : base(options) { }

        public DbSet<ImageObj> Images { get; set; }
    }
}