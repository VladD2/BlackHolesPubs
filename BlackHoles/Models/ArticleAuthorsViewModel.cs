using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Data;
using System.Data.Entity;

namespace BlackHoles.Models
{
  public class ArticleAuthorsViewModel
  {
    public List<Author> ArticleAuthors   { get; set; }
    public List<Author> AvailableAuthors { get; set; }

    public string ArticleAuthorIds => string.Join(", ", ArticleAuthors.Select(a => a.Id));
  }
}