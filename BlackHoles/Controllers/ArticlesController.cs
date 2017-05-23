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
      var authors = db.Articles.Include(a => a.Authors).Include(a => a.Owner).FilterByOwner(User).ToList();
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
      Article article = db.Articles.Include(a => a.Messages).Include(a => a.Authors).Include(a => a.Owner)
        .FilterByOwner(User)
        .SingleOrDefault(a => a.Id == id);
      if (article == null)
      {
        return HttpNotFound();
      }
      return View(article);
    }

    // GET: Articles/Create
    public ActionResult Create()
    {
      var newArticle = new Article();
      return ContinueEdit(newArticle);
    }

    // POST: Articles/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Specialty,IssueYear,IssueNumber,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds,CurrentMessageText,References,Agreed")] Article article)
    {
      var settings = Settings.Default;
      var isCreating = article.Id == 0;
      FillPrperties(article);

      if (string.IsNullOrWhiteSpace(article.AuthorsIds))
        return ContinueEdit(article);

      if (Request.Files.Count != 4)
        throw new ApplicationException("Invalid uploaded files count!");

      article.Created = article.Modified;
      article.Owner   = User.GetApplicationUser(db);
      article.Issue   = db.Issues.Find(settings.Year, settings.Number);

      if (article.Issue == null)
        throw new ApplicationException("Незаполнен список изданий!");

      DbUtils.Revalidate(this, article);
      if (ModelState.IsValid)
      {
        HttpPostedFileBase articleFile = Request.Files[0];
        HttpPostedFileBase additionalTextFile = Request.Files[1];
        HttpPostedFileBase additionalImg1 = Request.Files[2];
        HttpPostedFileBase additionalImg2 = Request.Files[3];

        if (isCreating && articleFile.ContentLength == 0)
        {
          ModelState.AddModelError("articleFile", "Необходимо загрузить файл содержащий текст статьи!");
          return ContinueEdit(article);
        }

        if (!CheckFiles(additionalImg1, additionalImg2))
          return ContinueEdit(article);

        TryAddMessage(article);

        db.Articles.Add(article);
        db.SaveChanges();

        if (articleFile.ContentLength > 0)
          SeveFile(article, articleFile, null);

        if (additionalTextFile.ContentLength > 0)
          SeveFile(article, additionalTextFile, ReviewTextPrefix);

        TrySeveFiles(article, additionalImg1, additionalImg2, ReviewImgPrefix);

        TrySendMessage(article);

        return RedirectToAction("Index");
      }

      article.AuthorsViewModel = MakeArticleAuthorsViewModel(article, article.AuthorsIds.ParseToIntArray());
      return View(article);
    }

    private void TryAddMessage(Article article)
    {
      if (!string.IsNullOrWhiteSpace(article.CurrentMessageText))
      {
        var user = User.GetApplicationUser(db);
        var msg = new Message { Created = DateTime.UtcNow, Text = article.CurrentMessageText, WriterId = user.Id, Writer = user, Messages = new List<Message>() };
        if (article.Messages == null)
          article.Messages = new List<Message>();
        article.Messages.Add(msg);
      }
    }

    private void TrySendMessage(Article article)
    {
      if (article.CurrentMessageText == null)
        return;

      MailMessageService.SendMail(MailMessageService.MainEmail, $"Коментраий к статье '{article.ShortArtTitles}'",
        $@"<html>
<body>
</body>
<p><i>Это автоматическое уведомление. <b>Не отвечайте</b> на него.</i> Для ответа перейдите по ссылке ниже.</p>
<p>Добавлен коментарий к статье <a href='{this.Action("Edit", "Articles", new { id = article.Id })}'>{article.ShortArtTitles}</a>, авторов: {article.GetAuthorsBriefFios()}.</p>

<p>Текст сообщения:<br />
{article.CurrentMessageText.Replace(Environment.NewLine, "<br />\r\n")}
</p>
</html>");
    }

    private void TrySendMessage(Article article, Message msg)
    {
      var parent = article.Messages.FindParentMessageOpt(msg);

      ApplicationUser parentWriter = parent == null ? article.Owner : parent.Writer;
      ApplicationUser writer = User.GetApplicationUser(db);

      MailMessageService.SendMail(parentWriter.Email, $"Коментраий к статье {article.GetAuthorsBriefFios()} '{article.ShortArtTitles}'",
        $@"<html>
<body>
</body>
<p><i>Это автоматическое уведомление. <b>Не отвечайте</b> на него.</i> Для ответа перейдите по ссылке ниже.</p>
<p>Добавлен коментарий к статье <a href='{this.Action("Edit", "Articles", new { id = article.Id })}'>{article.ShortArtTitles}</a>, авторов: {article.GetAuthorsBriefFios()}.</p>
<p>Автор: {writer.UserName} ({writer.Email})<br />
В ответ: {parentWriter.UserName} ({parentWriter.Email})</p>

<p>Текст сообщения:<br />
{msg.Text.Replace(Environment.NewLine, "<br />\r\n")}
</p>
</html>");
    }

    private bool CheckFiles(HttpPostedFileBase file1, HttpPostedFileBase file2)
    {
      if (file1.ContentLength == 0 && file2.ContentLength > 0)
      {
        ModelState.AddModelError("additionalImg", "Если задан дополнительный файл первый файл так же должен быть задан!");
        return false;
      }

      return true;
    }


    private void TrySeveFiles(Article article, HttpPostedFileBase file1, HttpPostedFileBase file2, string prefix = null)
    {
      if (file1.ContentLength > 0 && file2.ContentLength == 0)
        SeveFile(article, file1, prefix);
      else
      {
        SeveFile(article, file1, prefix);
        SeveFile(article, file2, prefix + "2-");
      }
    }

    private void SeveFile(Article article, HttpPostedFileBase file, string prefix = null)
    {
      var fullPath = MakeFileName(article, file, prefix);
      file.SaveAs(fullPath);
    }

    private string[] GetFileVersions(Article article, string prefix = null)
    {
      if (article.Id <= 0)
        return new string[0];

      var settings = Settings.Default;
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
      article.AuthorsViewModel = MakeArticleAuthorsViewModel(article, article.AuthorsIds.ParseToIntArray());
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
      if (article.References != null)
        article.References = article.References.Trim(' ', '\t', '\r', '\n');
    }

    void LoadNestedMessage(Message msg)
    {
      if (msg.Messages == null)
        db.Entry(msg).Collection(m => m.Messages).Load();

      if (msg.Writer == null)
        db.Entry(msg).Reference(m => m.Writer).Load();

      foreach (var subMsg in msg.Messages)
        LoadNestedMessage(subMsg);
    }

    // GET: Articles/Edit/5
    public ActionResult Edit(int? id)
    {
      if (id == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      var article = db.Articles
        .Include(a => a.Authors)
        .Include(a => a.Messages.Select(m => m.Writer))
        .FilterByOwner(User)
        .FirstOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      LoadNestedMessage(article);

      var authorsIds = article.Authors.Select(a => a.Id).ToArray();
      article.AuthorsIds = string.Join(", ", authorsIds);

      ArticleAuthorsViewModel newArticleAuthors = MakeArticleAuthorsViewModel(article, authorsIds);
      article.AuthorsViewModel = newArticleAuthors;
      FillFilesInfo(article);
      return View(article);
    }

    private void LoadNestedMessage(Article article)
    {
      foreach (var msg in article.Messages)
        LoadNestedMessage(msg);
    }

    // POST: Articles/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Specialty,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds,CurrentMessageText,References,Agreed,Status")] Article article)
    {
      var userId = User.GetUserId();
      var orig = db.Articles.Include(a => a.Authors).Include(a => a.Owner).Include(a => a.Issue)
                   .FilterByOwner(User).SingleOrDefault(a => a.Id == article.Id);
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

      return EditImpl(orig);
    }

    private ActionResult EditImpl(Article article)
    {
      FillPrperties(article);

      DbUtils.Revalidate(this, article);

      if (ModelState.IsValid)
      {
        HttpPostedFileBase articleFile = Request.Files[0];
        HttpPostedFileBase additionalTextFile = Request.Files[1];
        HttpPostedFileBase additionalImg1 = Request.Files[2];
        HttpPostedFileBase additionalImg2 = Request.Files[3];

        if (articleFile.ContentLength > 0)
          SeveFile(article, articleFile, null);

        if (additionalTextFile.ContentLength > 0)
          SeveFile(article, additionalTextFile, ReviewTextPrefix);

        if (!CheckFiles(additionalImg1, additionalImg2))
          return ContinueEdit(article);

        TrySeveFiles(article, additionalImg1, additionalImg2, ReviewImgPrefix);

        TryAddMessage(article);

        db.SaveChanges();

        TrySendMessage(article);

        return RedirectToAction("Index", "Home");
      }

      return ContinueEdit(article);
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

    private ArticleAuthorsViewModel MakeArticleAuthorsViewModel(Article article, int[] authorsIds)
    {
      var query = db.Authors.FilterByOwner(User);
      if (authorsIds != null && authorsIds.Length != 0)
        query = query.Where(a => !authorsIds.Contains(a.Id));
      var other = query.ToList();
      var newArticleAuthors = new ArticleAuthorsViewModel() { ArticleAuthors = article.Authors ?? new List<Author>(), AvailableAuthors = other };
      return newArticleAuthors;
    }

    // POST: Articles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
      Article article = db.Articles.Include(a => a.Messages).FilterByOwner(User)
        .SingleOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      LoadNestedMessage(article);

      foreach (var msg in article.Messages.ToArray())
        DeleteCascad(msg);

      article.Messages.Clear();

      db.Articles.Remove(article);
      db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult AddAuthorsAjax(List<int> autorIds)
    {
      autorIds = autorIds?.Distinct()?.ToList() ?? new List<int>();
      var autors = db.Authors
                    .FilterByOwner(User)
                    .Where(a => autorIds.Contains(a.Id))
                    .ToList();
      var autorsOrdered = (from autorId in autorIds join o in autors on autorId equals o.Id select o).ToList();
      var other = db.Authors.Include(a => a.Owner)
                    .FilterByOwner(User)
                    .Where(a => !autorIds.Contains(a.Id))
                    .OrderBy(a => a.RusSurname).ThenBy(a => a.RusInitials)
                    .ToList();
      var model = new ArticleAuthorsViewModel() { ArticleAuthors = autorsOrdered, AvailableAuthors = other };
      return PartialView("AuthorList", model);
    }

    [HttpPost]
    public ActionResult AddCommentAjax(Comment comment)
    {
      var userId = User.GetUserId();
      var writer = db.Users.Find(userId);
      if (writer == null)
        ModelState.AddModelError("commentsValidation", "Невозможно добавить комментарий, так как автор не вошел в систему!");
      else if (string.IsNullOrWhiteSpace(comment.Text))
        ModelState.AddModelError("commentsValidation", "Комментарий должен содержать текст!");
      else
      {
        var msg = new Message()
        {
          Created = DateTime.UtcNow,
          Text = comment.Text,
          Writer = writer,
          WriterId = userId,
        };

        var article = db.Articles
          .Include(a => a.Messages)
          .Include(a => a.Authors)
          .FilterByOwner(User)
          .FirstOrDefault(a => a.Id == comment.ArticleId);

        if (article == null)
          ModelState.AddModelError("commentsValidation", "Статья не найдена!");
        else
        {
          LoadNestedMessage(article);

          if (comment.ParentMsgId > 0)
          {
            var parentMsg = article.Messages.FindMessageOpt(comment.ParentMsgId);
            if (parentMsg == null)
              return HttpNotFound("Не выерные номера родительского сообщения!");

            parentMsg.Messages.Add(msg);
          }
          else
            article.Messages.Add(msg);

          TrySendMessage(article, msg);

          db.SaveChanges();
          ViewBag.ArticleId = comment.ArticleId;
          return PartialView("MessageTree", article.Messages.OrderByDescending(m => m.Created));
        }
      }
      return HttpNotFound("Не выерные номера сообщения или статьи!");
    }

    public ActionResult DeletCommentAjax(int articleId, int msgId)
    {
      var article = db.Articles
        .Include(a => a.Messages.Select(m => m.Writer))
        .FilterByOwner(User)
        .FirstOrDefault(a => a.Id == articleId);

      if (article == null)
        ModelState.AddModelError("commentsValidation", "Статья не найдена!");
      else
      {
        LoadNestedMessage(article);

        var msg = article.Messages.FindMessageOpt(msgId);
        if (msg == null)
          return HttpNotFound("Не выерные номера родительского сообщения!");

        DeleteCascad(msg);

        db.SaveChanges();
        ViewBag.ArticleId = articleId;
        return PartialView("MessageTree", article.Messages.OrderByDescending(m => m.Created));
      }
      return HttpNotFound("Не выерные номера сообщения!");
    }

    void DeleteCascad(Message msg)
    {
      if (msg.Messages != null)
        foreach (var subMsg in msg.Messages.ToArray())
          DeleteCascad(subMsg);

      db.Messages.Remove(msg);
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
