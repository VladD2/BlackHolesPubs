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

namespace BlackHoles.Controllers
{
  public class ArticlesController : Controller
  {
    private IssuesDb db = new IssuesDb();

    // GET: Articles
    public ActionResult Index()
    {
      var userId = User.GetUserId();
      return View(db.Articles.Where(a => a.Owner.Id == userId).ToList());
    }

    // GET: Articles/Details/5
    public ActionResult Details(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Article article = db.Articles.Find(id);
      if (article == null || article.Owner.Id != User.GetUserId())
      {
        return HttpNotFound();
      }
      return View(article);
    }

    // GET: Articles/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: Articles/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Specialty,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords")] Article article)
    {
      article.Owner = User.GetApplicationUser();
      if (ModelState.IsValid)
      {
        db.Articles.Add(article);
        db.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(article);
    }

    // GET: Articles/Edit/5
    public ActionResult Edit(int? id)
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

    [HttpPost]
    public ActionResult AddAuthors(int autorId)
    {
      return View();
    }

    [HttpPost]
    public ActionResult AddAuthorsAjax(List<int> autorIds)
    {
      var userId = User.GetUserId();
      autorIds = autorIds?.Distinct()?.ToList() ?? new List<int>();
      var autors = db.Authors.Include(a => a.Owner)
                    .Where(a => autorIds.Contains(a.Id) && a.Owner.Id == userId)
                    .ToList();
      var autorsOrdered = (from autorId in autorIds join o in autors on autorId equals o.Id select o).ToList();
      var other = db.Authors.Include(a => a.Owner)
                    .Where(a => !autorIds.Contains(a.Id) && a.Owner.Id == userId)
                    .OrderBy(a => a.RusSurname).ThenBy(a => a.RusInitials)
                    .ToList();
      var model = new AuthorListModel() { ArticleAuthors = autorsOrdered, AvailableAuthors = other };
      return PartialView("AuthorList", model);
    }

    // POST: Articles/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Specialty,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords")] Article article)
    {
      if (ModelState.IsValid)
      {
        db.Entry(article).State = EntityState.Modified;
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
