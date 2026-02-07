using ITI_Project.Core.Models.Location;
using ITI_Project.Core.Models.Moderation;
using ITI_Project.Core.Models.Persons;
using ITI_Project.Core.Models.Posts;
using ITI_Project.Core.Models.Requests;
using ITI_Project.Core.Models.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ITI_Project.Repository.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        // Persons
        public DbSet<User> Users { get; set; }
        public DbSet<Provider> Providers { get; set; }

        // Provider verification
        public DbSet<ProviderDocument> ProviderDocuments { get; set; }

        // Areas & locations
        public DbSet<Governorate> governorates { get; set; }
        public DbSet<Region> regions { get; set; }
        public DbSet<BaseLocation> BaseLocations { get; set; }
        public DbSet<LiveLocation> LiveLocations { get; set; }
        public DbSet<ServiceRequestLocation> ServiceRequestLocations { get; set; }

        // Services
        public DbSet<Service> Services { get; set; }
        public DbSet<ProviderService> ProviderServices { get; set; }
        public DbSet<ProviderContract> ProviderContracts { get; set; }

        // Requests & offers
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<RequestOffer> RequestOffers { get; set; }

        // Posts & media
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Reviews & reports
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Report> Reports { get; set; }

        // Admin
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }

        // User phones
        public DbSet<UserPhoneNumber> UserPhoneNumbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProviderContract>()
                .HasKey(pc => new { pc.ProviderId, pc.ServiceRequestId });

            modelBuilder.Entity<ProviderService>()
                .HasKey(ps => new { ps.ProviderId, ps.ServiceId });

            modelBuilder.Entity<ProviderService>()
                .HasOne(ps => ps.Provider)
                .WithMany(p => p.ProviderServices)
                .HasForeignKey(ps => ps.ProviderId);

            modelBuilder.Entity<ProviderService>()
                .HasOne(ps => ps.Service)
                .WithMany(s => s.ProviderServices)
                .HasForeignKey(ps => ps.ServiceId);

            base.OnModelCreating(modelBuilder);
        }

    }
}
