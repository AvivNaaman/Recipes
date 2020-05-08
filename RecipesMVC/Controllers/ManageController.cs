using RecipesMVC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecipesMVC.Controllers
{
    public class ManageController : Controller
    {

        // GET: Manage
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Index()
        {
            ConfigurationModel model = new ConfigurationModel(true);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Index(ConfigurationModel m)
        {
            var entities = new Models.EF_DB.RecipesAppEntities();
            if (m.SmtpEnabled)
            {
                if (m.Smtp.Password != null && m.Smtp.User != null && m.Smtp.Server != null)
                {
                    ConfigurationManager.AppSettings["SmtpUser"] = m.Smtp.User;
                    ConfigurationManager.AppSettings["SmtpPassword"] = m.Smtp.Password;
                    ConfigurationManager.AppSettings["SmtpServer"] = m.Smtp.Server;
                    ConfigurationManager.AppSettings["SmtpEnabled"] = m.SmtpEnabled.ToString();

                }
            }
            ConfigurationManager.AppSettings["SmtpUser"] = m.DarkMode.ToString();
            ConfigurationManager.AppSettings["SmtpUser"] = m.FontFamily;
            ConfigurationManager.AppSettings["SmtpUser"] = m.SiteName;
            entities.Dispose();
            return Json(new { Message = "Error" });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Users()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            var model = (from u in entities.Users select u).ToList();
            entities.Dispose();
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            if (entities.Users.Any(u => u.ID == id && !u.Role.Contains("Admin")))
            {
                var u = new RecipesMVC.Models.EF_DB.User { ID = id };
                entities.Users.Attach(u);
                entities.Users.Remove(u);
                entities.SaveChanges();
                entities.Dispose();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            entities.Dispose();
            return Json(new { Message = "ERROR", Error = "UserNotFound" }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ChangeRole(int id, string r)
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            if (entities.Users.Any(u => u.ID == id && !u.Role.Contains("Admin")))
            {
                var user = (from u in entities.Users where u.ID == id && !u.Role.Contains("Admin") select u).First();
                user.Role = r;
                entities.SaveChanges();
                entities.Dispose();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            entities.Dispose();
            return Json(new { Message = "ERROR", Error = "UserNotFound" }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Recipes()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            var modelInfo = (from r in entities.Recipes select r).ToList();
            var model = new List<RecipeForManage>();
            if (modelInfo.ToList().Count > 0)
            {
                foreach (var recipe in modelInfo.ToList().OrderByDescending(re => re.UploadedAt).ToList())
                {
                    var rToAdd = new RecipeForManage();
                    rToAdd.UploadedBy = recipe.User.UserName;
                    if (recipe.CategoryID.HasValue)
                    {
                        rToAdd.Category = recipe.Category.Name;
                    }
                    rToAdd.Title = recipe.Title;
                    if (recipe.UploadedAt.HasValue)
                    {
                        rToAdd.UploadedAt = recipe.UploadedAt.Value.ToString();
                    }
                    rToAdd.PublicStatus = recipe.publicStatus.Value;
                    rToAdd.ID = recipe.ID.ToString();
                    rToAdd.RouteURL = recipe.RouteURL;
                    model.Add(rToAdd);
                }
            }
            entities.Dispose();
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ChangeRecipePublic(int id)
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            if (entities.Recipes.Any(u => u.ID == id))
            {
                var rec = (from re in entities.Recipes where re.ID == id select re).First();
                if (rec.CategoryID.HasValue && rec.TotalTime.HasValue && rec.Title != null && rec.Ingredients.Contains(';') && rec.Instructions.Contains(';') && rec.KeyWords != null && rec.RecipeYield.HasValue)
                {
                    rec.publicStatus = !rec.publicStatus.Value;
                    entities.SaveChanges();
                    entities.Dispose();
                    return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    entities.Dispose();
                    return Json(new { Message = "Error", Error = "InfoNotFull" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Message = "Error", Error = "RecipeNotFound" }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteRecipe(int id)
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            if (entities.Recipes.Any(u => u.ID == id))
            {
                var u = new RecipesMVC.Models.EF_DB.Recipe { ID = id };
                entities.Recipes.Attach(u);
                entities.Recipes.Remove(u);
                entities.SaveChanges();
                entities.Dispose();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            entities.Dispose();
            return Json(new { Message = "ERROR", Error = "RecipeNotFound" }, JsonRequestBehavior.AllowGet);
        }
    }
    public class RecipeForManage
    {
        public string ID { get; set; }
        public string UploadedBy { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string UploadedAt { get; set; }
        public bool PublicStatus { get; set; }
        public string RouteURL { get; set; }
    }
}