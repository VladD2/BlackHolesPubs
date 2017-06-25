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
    private IssuesDb db = new IssuesDb();

    // GET: Articles
    public ActionResult Index()
    {
      var userId = User.GetUserId();
      var articlesQuery = db.Articles.Include(a => a.Authors).Include(a => a.Issue).Include(a => a.Owner).FilterByOwner(User);

      if (User.IsInRole(Constants.EditorRole))
        articlesQuery = articlesQuery.Where(a => a.Status != ArticleStatus.Published);

      articlesQuery = articlesQuery
        .OrderByDescending(a => a.IssueYear)
        .ThenByDescending (a => a.IssueNumber)
        .ThenByDescending (a => a.Status)
        .ThenBy           (a => a.Id);

      var articles = articlesQuery.ToList();

      foreach (var article in articles)
      {
        article.FillFilesInfo(Server.MapPath);
        article.Authors.Sort(DbUtils.AuthorComparison);
      }
      return View(articles);
    }

    // GET: Articles/Details/5
    public ActionResult Details(int? id)
    {
      if (id == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      Article article = db.Articles.Include(a => a.Messages).Include(a => a.Authors.Select(x => x.Owner)).Include(a => a.Authors).Include(a => a.Owner)
        .FilterByOwner(User)
        .SingleOrDefault(a => a.Id == id);

      if (article == null)
        return HttpNotFound();

      article.FillFilesInfo(Server.MapPath);
      article.Authors.Sort(DbUtils.AuthorComparison);

      return View(article);
    }

    // GET: Articles/Doc/5
    public ActionResult Doc(int? id)
    {
      return GetFile(id);
    }

    // GET: Articles/Doc/5
    public ActionResult ReviewTxt(int? id)
    {
      return GetFile(id, Constants.ReviewTextPrefix);
    }

    // GET: Articles/Doc/5
    public ActionResult ReviewImg(int? id)
    {
      return GetFile(id, Constants.ReviewImgPrefix);
    }

    // GET: Articles/Doc/5
    public ActionResult AntiplagiatApdx(int? id)
    {
      return GetFile(id, Constants.AntiplagiatApdxPrefix);
    }

    // GET: Articles/Doc/5
    public ActionResult AntiplagiatPdf(int? id)
    {
      return GetFile(id, Constants.AntiplagiatPdfPrefix);
    }

    private ActionResult GetFile(int? id, string prefix = null)
    {
      if (id == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      Article article = db.Articles.Include(a => a.Messages).Include(a => a.Authors.Select(x => x.Owner)).Include(a => a.Authors).Include(a => a.Owner)
        .FilterByOwner(User)
        .SingleOrDefault(a => a.Id == id);

      var path = article.GetNameForLatestFileVersion(Server.MapPath, prefix);
      var ext = Path.GetExtension(path);
      var name = Path.GetFileName(path);
      var contentType = "application/" + ext.TrimStart('.');
      return File(path, contentType, name);
    }

    // GET: Articles/Create
    public ActionResult Create()
    {
      ViewBag.Create = true;
      var newArticle = new Article();
      var issue = db.GetActiveIssue();
      newArticle.Issue = issue;
      newArticle.IssueYear = issue.Year;
      newArticle.IssueNumber = issue.Number;
      return ContinueEdit(newArticle);
    }

    // POST: Articles/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create([Bind(Include = "Id,Specialty,IssueYear,IssueNumber,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds,CurrentMessageText,References,Agreed,Status,ArticleDate")] Article article)
    {
      ViewBag.Create = true;
      var isCreating = article.Id == 0;
      article.FillPrperties(db);

      if (string.IsNullOrWhiteSpace(article.AuthorsIds))
        return ContinueEdit(article);

      if (Request.Files.Count != 4)
        throw new ApplicationException("Invalid uploaded files count!");

      article.Created = article.Modified;
      article.Owner   = User.GetApplicationUser(db);
      article.Issue   = db.Issues.Find(article.IssueYear, article.IssueNumber);

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

        ViewBag.Create = false;

        if (articleFile.ContentLength > 0)
          article.SeveFile(articleFile, Server.MapPath);

        if (additionalTextFile.ContentLength > 0)
          article.SeveFile(additionalTextFile, Server.MapPath, Constants.ReviewTextPrefix);

        article.TrySeveFiles(Server.MapPath, additionalImg1, additionalImg2, Constants.ReviewImgPrefix);

        TrySendMessage(article);

        return RedirectToAction("Details", new { id = article.Id });
      }

      return ContinueEdit(article);
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

      var user = User.GetApplicationUser(db);
      var email = user.Email.Equals(Constants.MainEmail, StringComparison.OrdinalIgnoreCase) ? article.Owner.Email : Constants.MainEmail;

      MailMessageService.SendMail(email, $"Коментраий к статье '{article.ShortArtTitles}'",
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
      if (file1?.ContentLength == 0 && file2?.ContentLength > 0)
      {
        ModelState.AddModelError("additionalImg", "Если задан дополнительный файл первый файл так же должен быть задан!");
        return false;
      }

      return true;
    }


    private ActionResult ContinueEdit(Article article)
    {
      article.MakeArticleAuthorsViewModel(User, db);
      article.FillFilesInfo(Server.MapPath);
      return View("ArticleView", article);
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

      ViewBag.Create = false;

      var article = db.Articles
        .Include(a => a.Authors)
        .Include(a => a.Messages.Select(m => m.Writer))
        .FilterByOwner(User)
        .FirstOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      LoadNestedMessage(article);

      article.Authors.Sort(DbUtils.AuthorComparison);
      article.MakeAuthorsIds();
      return ContinueEdit(article);
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
    public ActionResult Edit([Bind(Include = "Id,Specialty,IssueYear,IssueNumber,RusArtTitles,ShortArtTitles,RusAbstract,RusKeywords,EnuArtTitles,EnuAbstract,EnuKeywords,AuthorsIds,CurrentMessageText,References,Agreed,Status,ArticleDate")] Article article)
    {
      ViewBag.Create = false;

      var orig = db.Articles.Include(a => a.Authors).Include(a => a.Owner).Include(a => a.Issue)
                   .FilterByOwner(User).SingleOrDefault(a => a.Id == article.Id);
      if (orig == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      var prevStatus = orig.Status;

      Mapper.Initialize(cfg =>
        cfg.CreateMap<Article, Article>()
           .ForMember(dest => dest.Created,     opt => opt.Ignore())
           .ForMember(dest => dest.Owner,       opt => opt.Ignore())
           .ForMember(dest => dest.OwnerId,     opt => opt.Ignore())
           .ForMember(dest => dest.Authors,     opt => opt.Ignore())
           .ForMember(dest => dest.Modified,    opt => opt.Ignore())
           );

      Mapper.Map(article, orig);

      orig.Issue = db.Issues.Find(orig.IssueYear, orig.IssueNumber);

      return EditImpl(orig, prevStatus);
    }

    private ActionResult EditImpl(Article article, ArticleStatus prevStatus)
    {
      article.FillPrperties(db);

      DbUtils.Revalidate(this, article);

      if (ModelState.IsValid)
      {
        HttpPostedFileBase articleFile = Request.Files["articleFile"];
        HttpPostedFileBase additionalTextFile = Request.Files["additionalTextFile"];
        HttpPostedFileBase additionalImg1 = Request.Files["additionalImg1"];
        HttpPostedFileBase additionalImg2 = Request.Files["additionalImg2"];

        if (articleFile?.ContentLength > 0)
          SaveArticleFile(article, articleFile, prevStatus);

        if (additionalTextFile?.ContentLength > 0)
          article.SeveFile(additionalTextFile, Server.MapPath, Constants.ReviewTextPrefix);

        if (!CheckFiles(additionalImg1, additionalImg2))
          return ContinueEdit(article);

        article.TrySeveFiles(Server.MapPath, additionalImg1, additionalImg2, Constants.ReviewImgPrefix);

        TryAddAcceptedMessage(article, prevStatus);

        TryAddMessage(article);

        db.SaveChanges();

        TrySendMessage(article);

        return RedirectToAction("Details", new { id = article.Id });
      }

      return ContinueEdit(article);
    }

    private void SaveArticleFile(Article article, HttpPostedFileBase articleFile, ArticleStatus prevStatus)
    {
      var versionsOld = article.GetFileVersions(Server.MapPath);
      article.SeveFile(articleFile, Server.MapPath);

      if (prevStatus != ArticleStatus.RequiresVerification && article.Status != ArticleStatus.NewVersion)
      {
        article.Status = ArticleStatus.NewVersion;
        var versionsNew = article.GetFileVersions(Server.MapPath);

        MailMessageService.SendMail(Constants.MainEmail, $"Изменена версия файла к статье id{article.Id} '{article.ShortArtTitles}', авторов: {article.GetAuthorsBriefFios()}",
          $@"<html>
<body>
</body>
<p><i>Это автоматическое уведомление.</p>
<p>В статье <a href='{this.Action("Details", "Articles", new { id = article.Id })}'>{article.ShortArtTitles}</a>, авторов: <b>{article.GetAuthorsBriefFios()}</b> 
  был <b>изменен файл</b> статьи.
</p>
<p>Предыдущий статус: {prevStatus}</p>
</html>");
      }
    }

    private void TryAddAcceptedMessage(Article article, ArticleStatus prevStatus)
    {
      if (article.Status == ArticleStatus.Accepted && prevStatus != ArticleStatus.Accepted)
      {
        if (string.IsNullOrWhiteSpace(article.CurrentMessageText))
          article.CurrentMessageText = $@"Ваша статья принята к публикации в № {article.IssueNumber} за {article.IssueYear} год, который выйдет в июне.
При сдаче номера в печать Вы получите уведомление, содержащее: номера страниц, титульный лист, оглавление и PDF вашей статьи.
Реквизиты для оплаты публикации: http://www.k-press.ru/bh/Home/PaymentDetails
";

        MailMessageService.SendMail(Constants.ImposerEmail, $"Статья принята для печати. Сокращенное название: '{article.ShortArtTitles}'",
  $@"<html>
<body>
</body>
<p><i>Это автоматическое уведомление.</p>
<p>Статье <a href='{this.Action("Details", "Articles", new { id = article.Id })}'>{article.ShortArtTitles}</a>, авторов: {article.GetAuthorsBriefFios()} 
  <b>принята</b> к публикации в № {article.IssueNumber} за {article.IssueYear}.
</p>
</html>");
      }
      else if (article.Status == ArticleStatus.Paid && prevStatus != ArticleStatus.Paid)
      {
        if (string.IsNullOrWhiteSpace(article.CurrentMessageText))
          article.CurrentMessageText = $@"Получена оплата по статье <a href='{this.Action("Details", "Articles", new { id = article.Id })}'>{article.ShortArtTitles}</a>, авторов: {article.GetAuthorsBriefFios()}
";
      }
    }

    // GET: Articles/Delete/5
    public ActionResult Delete(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }
      Article article = db.Articles.FilterByOwner(User, Constants.AdminRole).SingleOrDefault(a => a.Id == id);
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
      Article article = db.Articles.Include(a => a.Messages).FilterByOwner(User, Constants.AdminRole).SingleOrDefault(a => a.Id == id);
      if (article == null)
        return HttpNotFound();

      if (article.Status == ArticleStatus.Accepted && !User.IsInRole(Constants.AdminRole))
        return HttpNotFound("Нельзя удалять принятую к публикации статью!");

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
          .Include(a => a.Authors.Select(au => au.Owner))
          .Include(a => a.Owner)
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

    // POST: Articles/UploadAntiplagiat/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UploadAntiplagiat([Bind(Include = "Id")] Article article)
    {
      Article orig = db.Articles.Include(a => a.Messages).Include(a => a.Authors.Select(x => x.Owner)).Include(a => a.Authors).Include(a => a.Owner).Include(a => a.Issue)
        .FilterByOwner(User)
        .SingleOrDefault(a => a.Id == article.Id);

      if (orig == null)
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

      HttpPostedFileBase antiplagiatApdx = Request.Files["antiplagiatApdx"];
      HttpPostedFileBase antiplagiatPdf  = Request.Files["antiplagiatPdf"];

      if (antiplagiatPdf?.ContentLength > 0 && antiplagiatApdx?.ContentLength > 0)
      {
        if (!Path.GetExtension(antiplagiatPdf.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
          ModelState.AddModelError("UploadAntiplagiatValidation", "В первом поле должен быть .pdf!");
          return View("Details", orig);
        }

        if (!Path.GetExtension(antiplagiatApdx.FileName).Equals(".apdx", StringComparison.OrdinalIgnoreCase))
        {
          ModelState.AddModelError("UploadAntiplagiatValidation", "Во втором поле должен быть .apdx!");
          return View("Details", orig);
        }

        orig.SeveFile(antiplagiatApdx, Server.MapPath, Constants.AntiplagiatApdxPrefix);
        orig.SeveFile(antiplagiatPdf, Server.MapPath, Constants.AntiplagiatPdfPrefix);

        MailMessageService.SendMail(orig.Owner.Email, $"Загружен отчет antiplagiat.ru к статье id{orig.Id} '{orig.ShortArtTitles}', автор{orig.AuthorsPlural}: {orig.GetAuthorsBriefFios()}",
          $@"<html>
<body>
<p><i>Это автоматическое уведомление.</p>
<p>Обратите внимание на то, что <b>статья еще не обработана редактором</b>. Это всего лишь оповещение о ходе обработки статьи. 
  Через некоторое время <b>Вам будет выслано уведомление о принятии статьи или письмо с замечаниями</b>, которые нужно исправить. 
</p>
<p>К статье '<a href='{this.Action("Details", "Articles", new { id = orig.Id })}'>{orig.ShortArtTitles}</a>', автор{orig.AuthorsPlural}: <b>{orig.GetAuthorsBriefFios()}</b> 
  был добавлен отчет Antiplagiat.ru.
</p>
<p>Вы можете скачать его <a href='{this.Action("AntiplagiatPdf", "Articles", new { id = orig.Id })}'>PDF-версию</a>.</p>
<p>Или его <a href='{this.Action("AntiplagiatApdx", "Articles", new { id = orig.Id })}'>APDX-версию</a>.</p>
<p>Для просмотра APDX-версии Вам придется загрузить <a href='http://www.antiplagiat.ru/Page/Antiplagiat-report-viewer'>Antiplagiat ReportViewer</a>.</p>
</body>
</html>");
        switch (orig.Status)
        {
          case ArticleStatus.RequiresVerification:
          case ArticleStatus.AddedToAntiplagiat:
          case ArticleStatus.NewVersion:
            orig.Status = ArticleStatus.AntiplagiatReportLoaded;
            orig.Agreed = true;
            db.SaveChanges();
            break;
        }

      }
      else
      {
        ModelState.AddModelError("UploadAntiplagiatValidation", "Вы дожны задать одноверменно обва варианта отчета antiplagiat.ru .apdx и .pdf!");
        return View("Details", orig);
      }

      return RedirectToAction("Details", new { id = article.Id });
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
