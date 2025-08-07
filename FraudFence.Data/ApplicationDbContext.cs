using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FraudFence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> Users { get; set; }
        public virtual DbSet<Article> Articles { get; set; }

        public virtual DbSet<Attachment> Attachments { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<ExternalAgency> ExternalAgencies { get; set; }

        public virtual DbSet<Newsletter> Newsletters { get; set; }

        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<PostAttachment> PostAttachments { get; set; }

        public virtual DbSet<ScamCategory> ScamCategories { get; set; }

        public virtual DbSet<ScamReport> ScamReports { get; set; }

        public virtual DbSet<ScamReportAttachment> ScamReportAttachments { get; set; }

        public virtual DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Article>().Navigation(x => x.User).AutoInclude();
            modelBuilder.Entity<Article>().Navigation(x => x.ScamCategory).AutoInclude();

            modelBuilder.Entity<Comment>().Navigation(x => x.User).AutoInclude();
            modelBuilder.Entity<Comment>().Navigation(x => x.Post).AutoInclude();

            modelBuilder.Entity<Newsletter>().Navigation(x => x.Articles).AutoInclude();

            modelBuilder.Entity<Post>().Navigation(x => x.User).AutoInclude();
            modelBuilder.Entity<Post>().Navigation(x => x.Comments).AutoInclude();
            modelBuilder.Entity<Post>().Navigation(x => x.PostAttachments).AutoInclude();

            modelBuilder.Entity<PostAttachment>().Navigation(x => x.Attachment).AutoInclude();

            modelBuilder.Entity<ScamReport>().Navigation(x => x.ScamCategory).AutoInclude();
            modelBuilder.Entity<ScamReport>().Navigation(x => x.ExternalAgency).AutoInclude();
            modelBuilder.Entity<ScamReport>().Navigation(x => x.ScamReportAttachments).AutoInclude();
            modelBuilder.Entity<ScamReport>().Navigation(x => x.Posts).AutoInclude();

            modelBuilder.Entity<ScamReportAttachment>().Navigation(x => x.Attachment).AutoInclude();

            modelBuilder.Entity<Setting>().Navigation(x => x.User).AutoInclude();
            modelBuilder.Entity<Setting>().Navigation(x => x.ScamCategory).AutoInclude();
            
            
            modelBuilder.Entity<ScamReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.SubmittedScamReports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add the new many-to-many config for reviewers:
            modelBuilder.Entity<ScamReport>()
                .HasMany(r => r.Reviewers)
                .WithMany(u => u.ReviewedScamReports);

            modelBuilder.Entity<Post>(e =>
            {
                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.UserId);
            });

            modelBuilder.Entity<Comment>(e =>
            {
                e.HasOne(x => x.Post)
                 .WithMany(x => x.Comments)
                 .HasForeignKey(x => x.PostId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ParentComment)
                 .WithMany()
                 .HasForeignKey(x => x.ParentCommentId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(x => x.PostId);
                e.HasIndex(x => x.UserId);
            });

            #region Data Seeding

            modelBuilder.Entity<ScamCategory>(e =>
            {
                e.HasData(
                    new ScamCategory { Id = 1, Name = "Banking Scams", ParentCategoryId = null },
                    new ScamCategory { Id = 2, Name = "Investment Scams", ParentCategoryId = null },
                    new ScamCategory { Id = 3, Name = "Shopping Scams", ParentCategoryId = null },
                    new ScamCategory { Id = 4, Name = "Social Media Scams", ParentCategoryId = null }
                );
            });

            #endregion
        }
    }
}
    