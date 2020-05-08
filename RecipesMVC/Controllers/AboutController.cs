using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;

namespace RecipesMVC.Controllers
{
    public class AboutController : Controller
    {
        // GET: About
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(RecipesMVC.Models.ContactModel m)
        {
            if (m.body != null && m.title != null && m.mailAddr != null)
            {
                try
                {
                    MailAddress Sender = new MailAddress(m.mailAddr);
                }
                catch
                {
                    return Json(new { Message = "ERROR", Error = "EmailInvalid" }, JsonRequestBehavior.AllowGet);
                }
                //TODO: Add sending mail logic HERE!
            }
            return Json(new { Message = "ERROR", Error = "Null" }, JsonRequestBehavior.AllowGet);
        }
    }
}