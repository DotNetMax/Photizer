using Microsoft.EntityFrameworkCore;
using Photizer.Domain.Entities;
using System;
using System.IO;

namespace Photizer.Infrastructure.Data
{
    public class PhotizerDbContext : DbContext
    {
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<Lense> Lenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData
                , Environment.SpecialFolderOption.DoNotVerify), "PhotizerData", "Photizer.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Picture>().HasOne(p => p.Lense).WithMany(l => l.Pictures);
            modelBuilder.Entity<Picture>().HasOne(p => p.Camera).WithMany(c => c.Pictures);
            modelBuilder.Entity<Picture>().HasOne(p => p.Location).WithMany(l => l.Pictures);
            modelBuilder.Entity<Picture>().HasOne(p => p.Category).WithMany(c => c.Pictures);

            modelBuilder.Entity<PictureTag>().HasKey(key => new { key.PictureId, key.TagId });
            modelBuilder.Entity<CollectionPicture>().HasKey(key => new { key.PictureId, key.CollectionId });
            modelBuilder.Entity<PicturePerson>().HasKey(key => new { key.PictureId, key.PersonId });

            modelBuilder.Entity<PictureTag>().HasOne(pt => pt.Picture).WithMany(p => p.PictureTags).HasForeignKey(pt => pt.PictureId);
            modelBuilder.Entity<PictureTag>().HasOne(pt => pt.Tag).WithMany(t => t.PictureTags).HasForeignKey(pt => pt.TagId);

            modelBuilder.Entity<CollectionPicture>().HasOne(cp => cp.Picture).WithMany(p => p.CollectionPictures).HasForeignKey(cp => cp.PictureId);
            modelBuilder.Entity<CollectionPicture>().HasOne(cp => cp.Collection).WithMany(c => c.CollectionPictures).HasForeignKey(cp => cp.CollectionId);

            modelBuilder.Entity<PicturePerson>().HasOne(pp => pp.Picture).WithMany(p => p.PicturePeople).HasForeignKey(pp => pp.PictureId);
            modelBuilder.Entity<PicturePerson>().HasOne(pp => pp.Person).WithMany(p => p.PicturePeople).HasForeignKey(pp => pp.PersonId);
        }
    }
}