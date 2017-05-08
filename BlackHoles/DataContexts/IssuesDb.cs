using BlackHoles.Entities;
using BlackHoles.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BlackHoles.DataContexts
{
  public class IssuesDb : IdentityDbContext<ApplicationUser>
  {
    public static IssuesDb Create() { return new IssuesDb(); }
    public IssuesDb() : base("DefaultConnection", throwIfV1Schema: false)
    {
    }
    
    public DbSet<Author>  Authors  { get; set; }
    public DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Article>()
                  .HasMany<Author>(s => s.Authors)
                  .WithMany(c => c.Articles)
                  .Map(cs =>
                    {
                      cs.MapLeftKey("AuthorRefId");
                      cs.MapRightKey("ArticleRefId");
                      cs.ToTable("ArticleAuthors");
                    });

      modelBuilder.Entity<Article>()
        .HasRequired(c => c.Owner)
        .WithMany()
        .WillCascadeOnDelete(false);

      //modelBuilder.Entity<Article>()
      //  .HasRequired(c => c.Authors)
      //  .WithMany()
      //  .WillCascadeOnDelete(false);

      base.OnModelCreating(modelBuilder);
    }

    public System.Data.Entity.DbSet<BlackHoles.Entities.Issue> Issues { get; set; }

    public System.Data.Entity.DbSet<BlackHoles.Entities.Message> Messages { get; set; }
  }
}