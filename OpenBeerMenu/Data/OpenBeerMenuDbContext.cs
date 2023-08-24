using Microsoft.EntityFrameworkCore;
using OpenBeerMenu.Data.Entities;

namespace OpenBeerMenu.Data
{
    public class OpenBeerMenuDbContext : DbContext
    {
        public DbSet<BeerInfo> Beers { get; set; }
        
        public DbSet<SectionInfo> Sections { get; set; }
        
        public DbSet<MenuInfo> Menus { get; set; }
        
        public DbSet<Settings> Settings { get; set; }

        public OpenBeerMenuDbContext(DbContextOptions<OpenBeerMenuDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SectionBeer>()
                .HasKey(sb => new { sb.SectionId, sb.BeerId});

            modelBuilder.Entity<SectionBeer>()
                .HasOne(sb => sb.Section)
                .WithMany(s => s.SectionBeers)
                .HasForeignKey(sb => sb.SectionId);
            
            modelBuilder.Entity<SectionBeer>()
                .HasOne(sb => sb.Beer)
                .WithMany(b => b.SectionBeers)
                .HasForeignKey(sb => sb.BeerId); 
            
            
            modelBuilder.Entity<MenuSection>()
                .HasKey(ms => new { ms.MenuId, ms.SectionId});

            modelBuilder.Entity<MenuSection>()
                .HasOne(ms => ms.Menu)
                .WithMany(m => m.MenuSections)
                .HasForeignKey(ms => ms.MenuId);
            
            modelBuilder.Entity<MenuSection>()
                .HasOne(ms => ms.Section)
                .WithMany(s => s.MenuSections)
                .HasForeignKey(ms => ms.SectionId);
        }
    }
}