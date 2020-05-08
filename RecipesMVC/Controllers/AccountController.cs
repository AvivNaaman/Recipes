using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipesMVC.Models;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;

namespace RecipesMVC.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        #region Profile Page
        [Authorize]
        public ActionResult Index()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            var model = (from u in entities.Users where u.UserName == User.Identity.Name select u).First();
            entities.Dispose();
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public ActionResult Change(RecipesMVC.Models.EF_DB.User m)
        {
            if (m.Email != null && m.UserName != null && m.BirthDay.HasValue)
            {
                var entities = new Models.EF_DB.RecipesAppEntities();
                var user = (from u in entities.Users where u.UserName == User.Identity.Name select u).First();
                user.UserName = m.UserName;
                user.Email = m.Email;
                user.BirthDay = m.BirthDay;
                user.FirstName = m.FirstName;
                entities.SaveChanges();
                entities.Dispose();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "Error", Error="InfoNotFull" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [Authorize]
        public ActionResult Delete()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            var u = new RecipesMVC.Models.EF_DB.User { UserName = User.Identity.Name };
            FormsAuthentication.SignOut();
            entities.Users.Attach(u);
            entities.Users.Remove(u);
            entities.SaveChanges();
            entities.Dispose();
            return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Authorize]
        public ActionResult ClearData()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            int uid = (from u in entities.Users where u.UserName == User.Identity.Name select u.ID).First();
            entities.Database.ExecuteSqlCommand("DELETE FROM Ratings WHERE UserID=" + uid);
            entities.Database.ExecuteSqlCommand("DELETE FROM Favorites WHERE UserID=" + uid);
            entities.Database.ExecuteSqlCommand("DELETE FROM Recipes WHERE UserID=" + uid);
            entities.SaveChanges();
            entities.Dispose();
            return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        [Authorize]
        public ActionResult Logout()
        {
            try
            {

                FormsAuthentication.SignOut();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return new HttpStatusCodeResult(500);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginModel Credentials)
        {
            if (Credentials.LoginUser != null && Credentials.LoginPwd != null)
            {
                List<string> ValidateNo = new List<string>();
                ValidateNo.Add("\'");
                ValidateNo.Add("\"");
                ValidateNo.Add("=");
                ValidateNo.Add("+");
                ValidateNo.Add("--");
                if (Credentials.LoginPwd.Any(word => Credentials.LoginUser.Contains(word)))
                {
                    if (Credentials.LoginPwd.Any(word => Credentials.LoginPwd.Contains(word)))
                    {
                        using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
                        {
                            var SubQuery = (from u in entities.Users where (u.UserName == Credentials.LoginUser && u.Password == Credentials.LoginPwd) select u);
                            try
                            {
                                var query = SubQuery.First();
                                if (query != null)
                                {
                                    FormsAuthentication.SetAuthCookie(query.UserName, Credentials.RememberMe);
                                    return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { Message = "ERROR", Error = "Credentials" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (InvalidOperationException) //= The row doesn't exist.
                            {
                                return Json(new { Message = "ERROR", Error = "Credentials" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { Message = "ERROR", Error = "InvalidChars" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Message = "ERROR", Error = "Null" });
        }
        #region Reset Password
        [HttpGet]
        public ActionResult Reset()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("~/Account");
            return View();
        }
        [HttpPost]
        public ActionResult Reset(ResetPwdModel m)
        {
            if (m.email != null)
            {
                if (IsValidEmail(m.email))
                {
                    using (var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities())
                    {
                        if (entities.Users.Any(u => u.Email == m.email))
                        {
                            //then.. email address valid.
                            Guid verCode = Guid.NewGuid();
                            var CurrUser = (from u in entities.Users where u.Email == m.email select u).FirstOrDefault();
                            CurrUser.ResetGUID = verCode.ToString();
                            CurrUser.ResetGUIDexp = DateTime.Now.AddDays(1);
                            entities.SaveChanges();
                            string[] emailInfo = new string[3];
                            emailInfo[0] = verCode.ToString();
                            emailInfo[1] = CurrUser.UserName;
                            emailInfo[2] = Request.Url.Scheme + "://" + Request.Url.Host;
                            RecipesMVC.Core.EmailMessaging msg = new Core.EmailMessaging(CurrUser.Email, Core.EmailMessaging.MessageType.ResetPassword, emailInfo);
                            msg.SendAsyncAndDispose();
                            return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
            }
            return Json(new { Message = "ERROR" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ResetV(string id)
        {
            if (id != null)
            {
                using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
                {
                    if (entities.Users.Any(u => u.ResetGUID == id))
                    {
                        return View();
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            else return HttpNotFound();
        }
        [HttpPost]
        [ActionName("ResetV")]
        public ActionResult ResetV(ResetVModel m)
        {
            if (m.id != null && m.pass != null && (m.pass == m.pass0))
            {
                using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
                {
                    if (entities.Users.Any(u => u.ResetGUID == m.id))
                    {
                        if (entities.Users.Any(u => u.ResetGUID == m.id && u.Password == m.pass))
                        {
                            return Json(new { Message = "ERROR", Error = "SameAsOld" }, JsonRequestBehavior.AllowGet);
                        }
                        var user = (from u in entities.Users where u.ResetGUID == m.id select u).FirstOrDefault();
                        user.ResetGUID = null;
                        user.ResetGUIDexp = null;
                        user.Password = m.pass;
                        entities.SaveChanges();
                        entities.Dispose();
                        return Json(new { Message = "SUCCESS"}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Message = "ERROR", Error = "FalseCode" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { Message ="ERROR", Error = "NotSame" }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        #endregion
        #region saved
        public ActionResult Saved()
        {
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            var Favorites = from f in entities.Favorites where f.User.UserName == User.Identity.Name && f.Recipe.publicStatus.Value orderby f.ID select f.Recipe;
            List<sRecipeFavorites> FavoritesArr = new List<sRecipeFavorites>();
            foreach (var recipe in Favorites)
            {
                FavoritesArr.Add(new sRecipeFavorites()
                {
                    Title = recipe.Title,
                    CategoryName = recipe.Category.Name,
                    UploadedBy = recipe.User.UserName,
                    Time = recipe.TotalTime.Value.ToString("hh\\:mm"),
                    RouteURL = recipe.RouteURL
                });
            }
            entities.Dispose();
            return View(FavoritesArr);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ChangeSavedStatus(string id)
        {
            if (id != null)
            {
                //id is routeurl.
                var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
                var currFitem = (from f in entities.Favorites where f.User.UserName == User.Identity.Name && f.Recipe.RouteURL == id select f).FirstOrDefault();
                if (currFitem != null)
                {
                    //then it's alreay written and needed to be changes.
                    entities.Favorites.Remove(currFitem);
                }
                else
                {
                    //let's add this!
                    var fav = new RecipesMVC.Models.EF_DB.Favorite()
                    {
                        RecipeID = (from re in entities.Recipes where re.RouteURL == id select re.ID).First(),
                        UserID = (from u in entities.Users where u.UserName == User.Identity.Name select u.ID).First()
                    };
                    var favsTbl = entities.Set<RecipesMVC.Models.EF_DB.Favorite>();
                    favsTbl.Add(fav);
                }
                entities.SaveChanges();
                entities.Dispose();
                return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
            }
            else return HttpNotFound();
        }
        #endregion
        public ActionResult RedirectHome()
        {
            return Redirect("~/");
        }
        [NonAction]
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
    public class sRecipeFavorites
    {
        public string Title { get; set; }
        public string UploadedBy { get; set; }
        public string Time { get; set; }
        public string CategoryName { get; set; }
        public string RouteURL { get; set; }
    }
}