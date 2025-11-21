using Microsoft.EntityFrameworkCore;
using Westmarch_tool.Core.Entities;
using Westmarch_tool.Core.Entities.Auth;
using Westmarch_tool.Core.Entities.Characters;

namespace Westmarch_tool.Infrastructure.Data
{
    public class WestmarchDbContext : DbContext
    {
        public WestmarchDbContext(DbContextOptions<WestmarchDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Player> Players { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<CharacterAttributes> CharacterAttributes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Player configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.DiscordId).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Character configuration
            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PlayerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Class).IsRequired().HasMaxLength(50);

                // Relationship with Player
                entity.HasOne(e => e.Player)
                    .WithMany(p => p.Characters)
                    .HasForeignKey(e => e.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship with CharacterAttributes
                entity.HasOne(e => e.Attributes)
                    .WithOne(a => a.Character)
                    .HasForeignKey<CharacterAttributes>(a => a.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CharacterAttributes configuration
            modelBuilder.Entity<CharacterAttributes>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CharacterId).IsUnique();
            });
        }
    }
}