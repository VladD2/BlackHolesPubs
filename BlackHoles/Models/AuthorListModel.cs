using BlackHoles.DataContexts;
using BlackHoles.Entities;
using BlackHoles.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Data;
using System.Data.Entity;

namespace BlackHoles.Models
{
  public class AuthorListModel
  {
    public List<Author> ArticleAuthors   { get; set; }
    public List<Author> AvailableAuthors { get; set; }

    public static AuthorListModel GetEmptyAuthorListModel(IssuesDb db, IPrincipal principal)
    {
      var userId = principal.GetUserId();
      var other = db.Authors.Include(a => a.Owner)
            .Where(a => a.Owner.Id == userId)
            .ToList();
      return new AuthorListModel() { ArticleAuthors = new List<Author>(), AvailableAuthors = other };
    }

    public string ArticleAuthorIds => string.Join(", ", ArticleAuthors.Select(a => a.Id));
  }
}