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

        // Requests & offers
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<RequestOffer> RequestOffers { get; set; }

        // Posts & media
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }

        // Reviews & reports
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Report> Reports { get; set; }

        // Admin
        public DbSet<AdminActionLog> AdminActionLogs { get; set; }

        // User phones
        public DbSet<UserPhoneNumber> UserPhoneNumbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* --------------------- Services Models -----------------  */
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

            /* --------------------- Posts & Media Models -----------------  */

            modelBuilder.Entity<PostReaction>()
                .HasKey(r => new { r.ServicePostId, r.UserId });

            modelBuilder.Entity<CommentReaction>()
                .HasKey(r => new { r.CommentId, r.UserId });

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostReaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.PostReactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommentReaction>()
                .HasOne(cr => cr.User)
                .WithMany(u => u.CommentReactions)
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            /* --------------------- Requests & Offers Models -----------------  */

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.FinalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RequestOffer>()
                .HasOne(o => o.Provider)
                .WithMany(p => p.RequestOffers)
                .HasForeignKey(o => o.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            /* --------------------- Areas & Locations Models -----------------  */
            modelBuilder.Entity<Provider>()
                .HasOne(p => p.Region)
                .WithMany(r => r.Providers)
                .HasForeignKey(p => p.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provider>()
                .HasOne(p => p.Governorate)
                .WithMany(g => g.Providers)
                .HasForeignKey(p => p.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            /* --------------------- Reviews & Reports Models -----------------  */

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.TargetUser)
                .WithMany()
                .HasForeignKey(r => r.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Resolver)
                .WithMany()
                .HasForeignKey(r => r.ResolverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Provider)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }

    }
}
