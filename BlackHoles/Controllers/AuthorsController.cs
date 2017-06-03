using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlackHoles.DataContexts;
using BlackHoles.Utils;
using AutoMapper;
using BlackHoles.Models;

namespace BlackHoles.Controllers
{
  [Authorize]
  public class AuthorsController : Controller
  {
    private IssuesDb db = new IssuesDb();

    // GET: Authors
    public ActionResult Index()
    {
      return View(db.Authors.Include(a => a.Owner).FilterByOwner(User).OrderBy(a => a.RusSurname).ToList());
    }

    // GET: Authors/Details/5
    public ActionResult Details(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Author author = db.Authors.Include(x => x.Owner).FilterByOwner(User).FirstOrDefault(a => a.Id == id);
      if (author == null)
      {
        return HttpNotFound();
      }
      return View(author);
    }

    // GET: Authors/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: Authors/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Email,RusSurname,RusInitials,RusOrgName,RusSubdivision,RusPosition,EnuSurname,EnuInitials,EnuOrgName,ScienceDegree,Phone,OrganizationKind", Exclude="Owner")] Author author)
    {
      var userId = User.GetUserId();
      var user = db.Users.Find(userId);
      author.Owner = user;

      this.Revalidate(author);

      if (ModelState.IsValid)
      {
        db.Authors.Add(author);
        db.SaveChanges();
        return RedirectToAction("Details", new { id = author.Id });
      }

      return View(author);
    }

    // GET: Authors/Edit/5
    public ActionResult Edit(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Author author = db.Authors.FilterByOwner(User).FirstOrDefault(a => a.Id == id);
      if (author == null)
      {
        return HttpNotFound();
      }
      return View(author);
    }

    // POST: Authors/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Email,RusSurname,RusInitials,RusOrgName,RusSubdivision,RusPosition,EnuSurname,EnuInitials,EnuOrgName,ScienceDegree,Phone,OrganizationKind")] Author author)
    {
      var userId = User.GetUserId();
      var orig = db.Authors.Include(a => a.Owner).FilterByOwner(User).FirstOrDefault(a => a.Id == author.Id);
      if (orig == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      Mapper.Initialize(cfg =>
          cfg.CreateMap<Author, Author>()
              .ForMember(dest => dest.Owner,    opt => opt.Ignore())
              .ForMember(dest => dest.OwnerId,  opt => opt.Ignore())
              .ForMember(dest => dest.Articles, opt => opt.Ignore())
              );

      Mapper.Map(author, orig);


      this.Revalidate(orig);

      if (ModelState.IsValid)
      {
        db.SaveChanges();
        return RedirectToAction("Details", new { id = author.Id });
      }
      return View(author);
    }

    // GET: Authors/Delete/5
    public ActionResult Delete(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Author author = db.Authors.FilterByOwner(User, Constants.AdminRole).SingleOrDefault(a => a.Id == id);
      if (author == null)
      {
        return HttpNotFound();
      }
      return View(author);
    }

    // POST: Authors/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
      Author author = db.Authors.FilterByOwner(User, Constants.AdminRole).SingleOrDefault(a => a.Id == id);
      db.Authors.Remove(author);
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
