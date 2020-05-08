using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecipesMVC.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }
        [ActionName("403")]
        public ActionResult Unauthorvized()
        {
            return View();
        }
        [ActionName("404")]
        public ActionResult NotFound()
        {
            return View();
        }
        [ActionName("500")]
        public ActionResult InternalServerError()
        {
            return View();
        }
    }
}