using HackathonApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackathonApp.Data.Data
{
    public class HackathonContext : DbContext
    {
        public DbSet<Project> Projects => Set<Project>();

        public HackathonContext(DbContextOptions<HackathonContext> options)
            : base(options)
        {
        }

        // Fallback for design-time / when options not passed
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback connection string (same as in appsettings.json)
                optionsBuilder.UseSqlite("Data Source=Hackathon.db");

            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var project = modelBuilder.Entity<Project>();

            project.ToTable("Projects");
            project.HasKey(p => p.Id);

            project.Property(p => p.TeamName)
                .IsRequired()
                .HasMaxLength(100);

            project.Property(p => p.ProjectName)
                .IsRequired()
                .HasMaxLength(120);

            project.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(50);

            project.Property(p => p.Captain)
                .IsRequired()
                .HasMaxLength(100);

            project.Property(p => p.EventDate)
                .IsRequired()
                .HasColumnType("date");

            project.Property(p => p.Score)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            project.Property(p => p.Members)
                .IsRequired();
        }
    }
}
