﻿using System;
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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using IOFile = System.IO.File;
using System.IO;

namespace BlackHoles.Controllers
{
  [Authorize]
  public class ArticlesController : Controller
  {
    const string ReviewTextPrefix = "review-text-";
    const string ReviewImgPrefix = "review-img-";
    private IssuesDb db = new IssuesDb();

    // GET: Articles
    public ActionResult Index()
    {
      var userId = User.GetUserId();
      var authors = db.Articles.Include(a => a.Authors).Where(a => a.OwnerId == userId).ToList();
      foreach (var author in authors)
        FillFilesInfo(author);
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
      //Session["uploadDirName"] = "Temp/" + Guid.NewGuid().ToString("D");

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
      var isCreating = article.Id == 0;
      FillPrperties(article);

      if (string.IsNullOrWhiteSpace(article.AuthorsIds))
      {
        article.AuthorsViewModel = MakeArticleAuthorsViewModel(User.GetUserId(), article, new int[0]);
        return View(article);
      }

      if (Request.Files.Count != 3)
        throw new ApplicationException("Invalid uploaded files count!");

      article.Created = article.Modified;
      article.Owner   = User.GetApplicationUser(db);
      article.Issue   = db.Issues.Find(article.IssueYear, article.IssueNumber);
      DbUtils.Revalidate(this, article);
      if (ModelState.IsValid)
      {

        HttpPostedFileBase articleFile        = Request.Files[0];
        HttpPostedFileBase additionalTextFile = Request.Files[1];
        HttpPostedFileBase additionalImg      = Request.Files[2];

        if (isCreating && articleFile.ContentLength == 0)
        {
          ModelState.AddModelError("articleFile", "Необходимо загрузить файл содержащий текст статьи!");
          return ContinueEdit(article);
        }

        db.Articles.Add(article);
        db.SaveChanges();

        if (articleFile.ContentLength > 0)
          SeveFile(article, articleFile, null);

        if (additionalTextFile.ContentLength > 0)
          SeveFile(article, additionalTextFile, ReviewTextPrefix);

        if (additionalImg.ContentLength > 0)
          SeveFile(article, additionalImg, ReviewImgPrefix);

        return RedirectToAction("Index");
      }

      article.AuthorsViewModel = MakeArticleAuthorsViewModel(User.GetUserId(), article, article.AuthorsIds.ParseToIntArray());
      return View(article);
    }

    private void SeveFile(Article article, HttpPostedFileBase file, string prefix = null)
    {
      var fullPath = MakeFileName(article, file, prefix);
      file.SaveAs(fullPath);
    }

    private string[] GetFileVersions(Article article, string prefix = null)
    {
      var settings = Settings.Default;
      if (article.Id == 0)
        throw new ArgumentException("Article not created yet!", nameof(article));

      var authors = GetArticleAuthorsSurnames(article);
      var filePattern = $"{prefix}{settings.Year}-{settings.Number}-id{article.Id}-v*";
      var versions = Directory.GetFiles(GetArticleDir(article), filePattern);
      return versions;
    }

    private string GetArticleAuthorsSurnames(Article article)
    {
      return string.Join("-", article.Authors.Select(a => ValidFileText(a.RusSurname)));
    }

    private string GetArticleDir(Article article)
    {
      var settings = Settings.Default;
      var articleDir = Server.MapPath($"~/App_Data/UploadedFiles/{settings.Year}/{settings.Number}/{article.Id}/");
      if (!Directory.Exists(articleDir))
        Directory.CreateDirectory(articleDir);
      return articleDir;
    }

    private string MakeFileName(Article article, HttpPostedFileBase file, string prefix = null)
    {
      var settings = Settings.Default;
      var versions = GetFileVersions(article, prefix);
      var rx       = new Regex(@"\d{4}-\d-id\d+-v(?<version>\d+)");
      var version  = versions.Length + 1;

      foreach (var versionFileName in versions)
      {
        var res = rx.Match(Path.GetFileName(versionFileName));
        if (res.Success)
        {
          var ver = int.Parse(res.Groups["version"].Value);
          if (ver + 1 > version)
            version = ver + 1;
        }
      }

      var authors  = GetArticleAuthorsSurnames(article);
      var ext      = Path.GetExtension(file.FileName);
      var fileName = $"{prefix}{settings.Year}-{settings.Number}-id{article.Id}-v{version}-{authors}-{article.ShortArtTitles}{ext}";
      var fullPath = Path.Combine(GetArticleDir(article), fileName);
      return fullPath;
    }

    private void FillFilesInfo(Article article)
    {
      article.ArticleVersions = GetFileVersions(article).Length;
      article.ReviewTextVersions = GetFileVersions(article, ReviewTextPrefix).Length;
      article.ReviewImgVersions = GetFileVersions(article, ReviewImgPrefix).Length;
    }

    public static string ValidFileText(string text)
    {
      var builder = new StringBuilder(text.Length);
      foreach (var ch in text)
      {
        if (char.IsLetterOrDigit(ch))
          builder.Append(ch);
        else if (char.IsWhiteSpace(ch) || ch == '_')
          builder.Append('-');
      }

      return builder.ToString();
    }

    private ActionResult ContinueEdit(Article article)
    {
      article.AuthorsViewModel = MakeArticleAuthorsViewModel(User.GetUserId(), article, article.AuthorsIds.ParseToIntArray());
      FillFilesInfo(article);
      return View(article);
    }

    private void FillPrperties(Article article)
    {
      var authorsIds = article.AuthorsIds.ParseToIntArray();
      if (article.Authors == null)
        article.Authors = new List<Author>();
      else
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

      var article = db.Articles.Include(a => a.Authors).Include(a => a.Messages).Where(a => a.OwnerId == userId).FirstOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      var authorsIds = article.Authors.Select(a => a.Id).ToArray();
      article.AuthorsIds = string.Join(", ", authorsIds);

      ArticleAuthorsViewModel newArticleAuthors = MakeArticleAuthorsViewModel(userId, article, authorsIds);
      article.AuthorsViewModel = newArticleAuthors;
      FillFilesInfo(article);
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
           .ForMember(dest => dest.Created, opt => opt.Ignore())
           .ForMember(dest => dest.Owner, opt => opt.Ignore())
           .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
           .ForMember(dest => dest.Issue, opt => opt.Ignore())
           .ForMember(dest => dest.IssueYear, opt => opt.Ignore())
           .ForMember(dest => dest.IssueNumber, opt => opt.Ignore())
           .ForMember(dest => dest.Authors, opt => opt.Ignore())
           .ForMember(dest => dest.Modified, opt => opt.Ignore())
           );

      Mapper.Map(article, orig);

      FillPrperties(orig);

      //var entry = db.Entry(orog);
      //entry.Reference(a => a.Owner).Load();
      //entry.Reference(a => a.Issue).Load();
      DbUtils.Revalidate(this, orig);

      if (ModelState.IsValid)
      {
        HttpPostedFileBase articleFile = Request.Files[0];
        HttpPostedFileBase additionalTextFile = Request.Files[1];
        HttpPostedFileBase additionalImg = Request.Files[2];

        if (articleFile.ContentLength > 0)
          SeveFile(orig, articleFile, null);

        if (additionalTextFile.ContentLength > 0)
          SeveFile(orig, additionalTextFile, ReviewTextPrefix);

        if (additionalImg.ContentLength > 0)
          SeveFile(orig, additionalImg, ReviewImgPrefix);

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

    private ArticleAuthorsViewModel MakeArticleAuthorsViewModel(string userId, Article article, int[] authorsIds)
    {
      var query = db.Authors.Where(a => a.OwnerId == userId);
      if (authorsIds != null && authorsIds.Length != 0)
        query = query.Where(a => !authorsIds.Contains(a.Id));
      var other = query.ToList();
      var newArticleAuthors = new ArticleAuthorsViewModel() { ArticleAuthors = article.Authors, AvailableAuthors = other };
      return newArticleAuthors;
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

    //[HttpPost]
    //public ActionResult UploadArticle(HttpPostedFileBase[] files)
    //{
    //  if (files == null)
    //    return View();
    //
    //  var settings = Settings.Default;
    //  var uploadDirName = (string)Session["uploadDirName"];
    //
    //  if (uploadDirName == null)
    //    return HttpNotFound();
    //
    //  var dir = Server.MapPath("~/App_Data/UploadedFiles/" + settings.Year + "/" + settings.Number + "/" + uploadDirName);
    //  if (!System.IO.Directory.Exists(dir))
    //    System.IO.Directory.CreateDirectory(dir);
    //  var path = System.IO.Path.Combine(dir, fileInput.FileName);
    //  fileInput.SaveAs(path);
    //  return View();
    //}

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
