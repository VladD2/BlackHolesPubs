using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlackHoles.DataContexts;
using BlackHoles.Entities;
using BlackHoles.Utils;
using BlackHoles.Models;
using BlackHoles.Properties;
using AutoMapper;

namespace BlackHoles.Controllers
{
  public class ArticlesController : Controller
  {
    private IssuesDb db = new IssuesDb();

    // GET: Articles
    public ActionResult Index()
    {
      var userId = User.GetUserId();
      var authors = db.Articles.Include(a => a.Authors).Where(a => a.OwnerId == userId).ToList();
      return View(authors);
    }

    // GET: Articles/Details/5
    public ActionResult Details(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Article article = db.Articles.Find(id);
      if (article == null || article.OwnerId != User.GetUserId())
      {
        return HttpNotFound();
      }
      return View(article);
    }

    // GET: Articles/Create
    public ActionResult Create()
    {
      var settings = Settings.Default;
      var userId = User.GetUserId();
      var other = db.Authors.Where(a => a.OwnerId == userId).ToList();
      var newArticleAuthors = new ArticleAuthorsViewModel() { ArticleAuthors = new List<Author>(), AvailableAuthors = other };
      var now = DateTime.UtcNow;
      var newArticle = new Article()
      {
        AuthorsViewModel = newArticleAuthors,
        IssueYear        = settings.Year,
        IssueNumber      = settings.Number,
        Created          = now,
      };
      return View(newArticle);
    }

    // POST: Articles/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Specialty,IssueYear,IssueNumber,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds")] Article article)
    {
      FillPrperties(article);
      article.Owner = User.GetApplicationUser(db);
      article.Issue = db.Issues.Find(article.IssueYear, article.IssueNumber);
      DbUtils.Revalidate(this, article);
      if (ModelState.IsValid)
      {
        db.Articles.Add(article);
        db.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(article);
    }

    private void FillPrperties(Article article)
    {
      var authorsIds = article.AuthorsIds.ParseToIntArray();
      article.Authors.Clear();
      var authors = db.Authors.Where(a => authorsIds.Contains(a.Id)).ToList();
      article.Authors.AddRange(authors);
      article.Modified = DateTime.UtcNow;
    }

    // GET: Articles/Edit/5
    public ActionResult Edit(int? id)
    {
      if (id == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      var userId = User.GetUserId();

      var article = db.Articles.Include(a => a.Authors).Where(a => a.OwnerId == userId).FirstOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      var authorsIds = article.Authors.Select(a => a.Id).ToArray();
      article.AuthorsIds = string.Join(", ", authorsIds);

      var other = db.Authors.Where(a => a.OwnerId == userId && !authorsIds.Contains(a.Id)).ToList();
      var newArticleAuthors = new ArticleAuthorsViewModel() { ArticleAuthors = article.Authors, AvailableAuthors = other };
      article.AuthorsViewModel = newArticleAuthors;
      return View(article);
    }

    // POST: Articles/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Specialty,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds")] Article article)
    {
      var userId = User.GetUserId();
      var orig = db.Articles.Include(a => a.Authors).Include(a => a.Owner).Include(a => a.Issue).FirstOrDefault(a => a.Id == article.Id && a.OwnerId == userId);
      if (orig == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


      Mapper.Initialize(cfg =>
        cfg.CreateMap<Article, Article>()
           .ForMember(dest => dest.Created,     opt => opt.Ignore())
           .ForMember(dest => dest.Owner,       opt => opt.Ignore())
           .ForMember(dest => dest.OwnerId,     opt => opt.Ignore())
           .ForMember(dest => dest.Issue,       opt => opt.Ignore())
           .ForMember(dest => dest.IssueYear,   opt => opt.Ignore())
           .ForMember(dest => dest.IssueNumber, opt => opt.Ignore())
           .ForMember(dest => dest.Authors,     opt => opt.Ignore())
           .ForMember(dest => dest.Modified,    opt => opt.Ignore())
           );

      Mapper.Map(article, orig);

      FillPrperties(orig);

      //var entry = db.Entry(orog);
      //entry.Reference(a => a.Owner).Load();
      //entry.Reference(a => a.Issue).Load();
      DbUtils.Revalidate(this, orig);

      if (ModelState.IsValid)
      {
        //db.Entry(article).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(article);
    }

    // GET: Articles/Delete/5
    public ActionResult Delete(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Article article = db.Articles.Find(id);
      if (article == null)
      {
        return HttpNotFound();
      }
      return View(article);
    }

    // POST: Articles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
      Article article = db.Articles.Find(id);
      db.Articles.Remove(article);
      db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult AddAuthorsAjax(List<int> autorIds)
    {
      var userId = User.GetUserId();
      autorIds = autorIds?.Distinct()?.ToList() ?? new List<int>();
      var autors = db.Authors
                    .Where(a => autorIds.Contains(a.Id) && a.OwnerId == userId)
                    .ToList();
      var autorsOrdered = (from autorId in autorIds join o in autors on autorId equals o.Id select o).ToList();
      var other = db.Authors.Include(a => a.Owner)
                    .Where(a => !autorIds.Contains(a.Id) && a.OwnerId == userId)
                    .OrderBy(a => a.RusSurname).ThenBy(a => a.RusInitials)
                    .ToList();
      var model = new ArticleAuthorsViewModel() { ArticleAuthors = autorsOrdered, AvailableAuthors = other };
      return PartialView("AuthorList", model);
    }

    [HttpPost]
    public ActionResult UploadArticle(HttpPostedFileBase fileInput)
    {
      var dir = Server.MapPath("~/App_Data/UploadedFiles/");
      if (!System.IO.Directory.Exists(dir))
        System.IO.Directory.CreateDirectory(dir);
      var path = System.IO.Path.Combine(dir, fileInput.FileName);
      fileInput.SaveAs(path);
      return View();
    }

    //[HttpPost]
    //public ActionResult UploadArticle()
    //{
    //  return PartialView("AuthorList");
    //}

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}
