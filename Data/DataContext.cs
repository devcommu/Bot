using DevCommuBot.Data.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace DevCommuBot.Data
{
    internal class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Data.db");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserWarning>()
                .HasKey(uc => new { uc.UserId, uc.WarningId });
            modelBuilder.Entity<UserWarning>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.Warnings)
                .HasForeignKey(uw => uw.UserId);
        }
    }
}
