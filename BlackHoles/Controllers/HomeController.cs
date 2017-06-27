using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlackHoles.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()                   { return View(); }
    public ActionResult Oferta()                  { return View(); }
    public ActionResult Requirements()            { return View(); }
    public ActionResult FormattingRequirements()  { return View(); }
    public ActionResult FormattingExamples()      { return View(); }
    public ActionResult Conditions()              { return View(); }
    public ActionResult Prices()                  { return View(); }
    public ActionResult Antiplagiat()             { return View(); }
    public ActionResult ReviewRules()             { return View(); }
    public ActionResult Instruction()             { return View(); }

    public ActionResult About()
    {
      ViewBag.Message = "Об издательстве";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Контакты";

      return View();
    }
  }
}