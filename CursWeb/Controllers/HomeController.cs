using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CursSearchEngine;
namespace CursWeb.Controllers
{
    public class Result
    {
        public string doc;
    }
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            ViewData["test"] = "Введите запрос";
            return View(ViewData);
        }
        [HttpGet]
        public ViewResult Search(string textq = "Пустой запрос", int page = 1)
        {

            string a = textq;
            if (a != null && a != "")
            {
                var v = SearchEngine.Engine.Search(a, page);
                ViewData["result"] = v.num.ToString();
                var res = new List<Result>();
                ViewBag.ls = v;
                return View(ViewData);

            }
            return View(ViewBag);

        }
        public ActionResult AutocompleteSearch(string term)
        {
            List<string> words = SearchEngine.Engine.AutoComplete(term);
            return Json(words, JsonRequestBehavior.AllowGet);
        }
    }
}