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

namespace BlackHoles.Controllers
{
  [Authorize(Roles = Constants.AdminRole)]
  public class IssuesController : Controller
  {
    private IssuesDb db = new IssuesDb();

    // GET: Issues
    public ActionResult Index()
    {
      return View(db.Issues.ToList());
    }

    // GET: Issues/Details/5
    public ActionResult Details(int? year, int? number)
    {
      if (year == null || number == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Issue issue = db.Issues.Find(year, number);
      if (issue == null)
      {
        return HttpNotFound();
      }
      return View(issue);
    }

    // GET: Issues/Create
    public ActionResult Create()
    {
      return View();
    }

    // POST: Issues/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Year,Number,Active")] Issue issue)
    {
      if (ModelState.IsValid)
      {
        db.Issues.Add(issue);
        db.SaveChanges();
        return RedirectToAction("Index");
      }

      return View(issue);
    }

    // GET: Issues/Edit/5
    public ActionResult Edit(int? year, int? number)
    {
      if (year == null || number == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Issue issue = db.Issues.Find(year, number);
      if (issue == null)
      {
        return HttpNotFound();
      }
      return View(issue);
    }

    // POST: Issues/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Year,Number,Active")] Issue issue)
    {
      if (ModelState.IsValid)
      {
        db.Entry(issue).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("Index");
      }
      return View(issue);
    }

    // GET: Issues/Delete/5
    public ActionResult Delete(int? year, int? number)
    {
      if (year == null || number == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Issue issue = db.Issues.Find(year, number);
      if (issue == null)
      {
        return HttpNotFound();
      }
      return View(issue);
    }

    // POST: Issues/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int year, int number)
    {
      Issue issue = db.Issues.Find(year, number);
      db.Issues.Remove(issue);
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
