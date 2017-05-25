using BlackHoles.DataContexts;
using BlackHoles.Properties;
using BlackHoles.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BlackHoles.Models
{
  public class Article
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(8, ErrorMessage = Error.ToLong), Display(Name="№ специальности")]
    public string Specialty { get; set; }

    [Display(Name = "Создан")]
    public DateTime Created { get; set; }

    [Display(Name = "Изменен")]
    public DateTime Modified { get; set; }

    [ForeignKey("Issue"), Column(Order = 1)]
    public int IssueYear { get; set; }

    [ForeignKey("Issue"), Column(Order = 2)]
    public int IssueNumber { get; set; }

    [Required(ErrorMessage = Error.NotSet)]
    public Issue Issue { get; set; }

    [ForeignKey("Owner")]
    public string OwnerId { get; set; }

    [Required(ErrorMessage = Error.NotSet)]
    public ApplicationUser Owner { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(600, ErrorMessage = Error.ToLong), Display(Name="Заголовок")]
    public string RusArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), StringLength(50, MinimumLength = 3, ErrorMessage = Error.ToLong), Display(Name = "Краткое название")]
    [RegularExpression(@"^[^*|:""<>?/\\]+$", ErrorMessage = @"Краткое название статьи не должно содежать символы '^', '*', '|', '\', ':', '""', '<', '>', '?' и '/'!")]
    public string ShortArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(2000, MinimumLength=50, ErrorMessage = Error.ToLong), Display(Name = "Аннотация")]
    public string RusAbstract { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(255, MinimumLength=3, ErrorMessage = Error.ToLong), Display(Name = "Ключевые слова")]
    public string RusKeywords { get; set; }



    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(400, ErrorMessage = Error.ToLong), Display(Name = "Название статьи")]
    public string EnuArtTitles { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(2000, MinimumLength = 50, ErrorMessage = Error.ToLong), Display(Name = "Аннотация")]
    public string EnuAbstract { get; set; }

    [Required(ErrorMessage = Error.NotSet), DataType(DataType.MultilineText), StringLength(255, MinimumLength = 3, ErrorMessage = Error.ToLong), Display(Name = "Ключевые слова")]
    public string EnuKeywords { get; set; }

    [DataType(DataType.MultilineText), StringLength(8000, ErrorMessage = Error.ToLong), Display(Name = "Список литературы")]
    public string References { get; set; }

    [Required(ErrorMessage = Error.NotSet), Display(Name = "Автор(ы)")]
    public List<Author> Authors { get; set; }

    public List<Message> Messages { get; set; }

    [Display(Name = "Статус")]
    public ArticleStatus Status { get; set; }

    /// <summary>Используется для хранения текста сообщения при создании или записи статить.</summary>
    [NotMapped, DataType(DataType.MultilineText)]
    public string CurrentMessageText { get; set; }

    [NotMapped]
    public string AuthorsIds { get; set; }

    [NotMapped]
    public ArticleAuthorsViewModel AuthorsViewModel { get; set; }

    [NotMapped]
    public int ArticleVersions { get; set; }

    [NotMapped]
    public int ReviewTextVersions { get; set; }

    [NotMapped]
    public int ReviewImgVersions { get; set; }

    [NotMapped]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Без подтверждения согласия вы не можете добавить запрос на публикацию статьи!")]
    //[RegularExpression("true", ErrorMessage = "Вы обязаны принять условия, чтобы добавить запрос на публикацию статьи!")]
    public bool Agreed { get; set; }

    public string GetAuthorsBriefFios()
    {
      return string.Join(", ", this.Authors.Select(a => a.MakeBriefFio()));
    }

    public void FillPrperties(IssuesDb db)
    {
      var authorsIds = this.AuthorsIds.ParseToIntArray();
      if (this.Authors == null)
        this.Authors = new List<Author>();
      else
        this.Authors.Clear();
      var authors = db.Authors.Where(a => authorsIds.Contains(a.Id)).ToList();
      this.Authors.AddRange(authors);
      this.Modified = DateTime.UtcNow;
      if (this.References != null)
        this.References = this.References.Trim(' ', '\t', '\r', '\n');
    }

    public void MakeAuthorsIds()
    {
      var authorsIds = this.Authors.Select(a => a.Id);
      var newAuthorsIds = string.Intern(string.Join(", ", authorsIds));
      if (this.AuthorsIds != newAuthorsIds)
        this.AuthorsIds = newAuthorsIds;
    }

    public void FillFilesInfo(Func<string, string> mapPath)
    {
      this.ArticleVersions = this.GetFileVersions(mapPath).Length;
      this.ReviewTextVersions = this.GetFileVersions(mapPath, Constants.ReviewTextPrefix).Length;
      this.ReviewImgVersions = this.GetFileVersions(mapPath, Constants.ReviewImgPrefix).Length;
    }

    private string[] GetFileVersions(Func<string, string> mapPath, string prefix = null)
    {
      if (this.Id <= 0)
        return new string[0];

      var settings = Settings.Default;
      var authors = this.GetArticleAuthorsSurnames();
      var filePattern = $"{prefix}{settings.Year}-{settings.Number}-id{this.Id}-v*";
      var versions = Directory.GetFiles(this.GetArticleDir(mapPath), filePattern);
      return versions;
    }

    private string GetArticleAuthorsSurnames()
    {
      return string.Join("-", this.Authors.Select(a => ValidFileText(a.RusSurname)));
    }

    private string GetArticleDir(Func<string, string> mapPath)
    {
      var settings = Settings.Default;
      var articleDir = mapPath($"~/App_Data/UploadedFiles/{settings.Year}/{settings.Number}/{this.Id}/");
      if (!Directory.Exists(articleDir))
        Directory.CreateDirectory(articleDir);
      return articleDir;
    }

    public void TrySeveFiles(Func<string, string> mapPath, HttpPostedFileBase file1, HttpPostedFileBase file2, string prefix = null)
    {
      if (file1.ContentLength > 0 && file2.ContentLength == 0)
        SeveFile(file1, mapPath, prefix);
      else
      {
        SeveFile(file1, mapPath, prefix);
        SeveFile(file2, mapPath, prefix + "2-");
      }
    }

    public void SeveFile(HttpPostedFileBase file, Func<string, string> mapPath, string prefix = null)
    {
      var fullPath = MakeFileName(file, mapPath, prefix);
      file.SaveAs(fullPath);
    }

    public string MakeFileName(HttpPostedFileBase file, Func<string, string> mapPath, string prefix = null)
    {
      var settings = Settings.Default;
      var versions = GetFileVersions(mapPath, prefix);
      var rx = new Regex(@"\d{4}-\d-id\d+-v(?<version>\d+)");
      var version = versions.Length + 1;

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

      var authors = GetArticleAuthorsSurnames();
      var ext = Path.GetExtension(file.FileName);
      var fileName = $"{prefix}{settings.Year}-{settings.Number}-id{Id}-v{version}-{authors}-{ShortArtTitles}{ext}";
      var fullPath = Path.Combine(GetArticleDir(mapPath), fileName);
      return fullPath;
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

    public void MakeArticleAuthorsViewModel(IPrincipal user, IssuesDb db)
    {
      MakeArticleAuthorsViewModel(user, db, this.AuthorsIds.ParseToIntArray());
    }

    private void MakeArticleAuthorsViewModel(IPrincipal user, IssuesDb db, int[] authorsIds)
    {
      var query = db.Authors.FilterByOwner(user);
      if (authorsIds != null && authorsIds.Length != 0)
        query = query.Where(a => !authorsIds.Contains(a.Id));
      var other = query.ToList();
      var newArticleAuthors = new ArticleAuthorsViewModel()
      {
        ArticleAuthors = this.Authors ?? new List<Author>(),
        AvailableAuthors = other
      };
      this.AuthorsViewModel = newArticleAuthors;
    }

    public override string ToString()
    {
      var authors = Authors != null ? string.Join(", ", Authors.Select(a => a.RusSurname)) : "";
      return $"Id={Id} {authors}: {ShortArtTitles}";
    }
  }
}
