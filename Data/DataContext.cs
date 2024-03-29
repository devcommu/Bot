﻿using DevCommuBot.Data.Models.Forums;
using DevCommuBot.Data.Models.Giveaways;
using DevCommuBot.Data.Models.Users;
using DevCommuBot.Data.Models.Warnings;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

namespace DevCommuBot.Data
{
    internal class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<UserWarning> UserWarnings { get; set; }
        public DbSet<StarboardEntry> Starboards { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<ForumEntry> ForumEntries { get; set; }
        public DbSet<Giveaway> Giveaways { get; set; }

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
            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Entries);
            modelBuilder.Entity<User>()
                .Property(u => u.BoosterAdvantage)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<BoosterAdvantage?>(v));
        }
    }
}