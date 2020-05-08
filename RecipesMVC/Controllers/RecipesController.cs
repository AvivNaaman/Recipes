using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace RecipesMVC.Controllers
{
    public class RecipesController : Controller
    {
        [HttpGet]
        // GET: Recipes
        public ActionResult Index()
        {
            using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
            {
                ViewBag.Categories = (from c in entities.Categories select c.Name).ToList();
            }
            return View();
        }
        [HttpGet]
        public ActionResult GetSearchResults(string s, string c, string u, string q)
        {
            //s => sort => orderby
            //c => category
            //u => username
            //q => query => is result contains value
            var entities = new RecipesMVC.Models.EF_DB.RecipesAppEntities();
            IQueryable<Models.EF_DB.Recipe> SearchQ;
            if (!(s != null))
            {
                s = "r";
            }
            switch (s.ToLower()) //Get the rows sorted
            {
                case "d": //Date uploaded
                    SearchQ = from re in entities.Recipes
                              where re.publicStatus == true
                              orderby re.UploadedAt descending
                              select re;
                    break;
                case "t": //total work Time
                    SearchQ = from re in entities.Recipes
                              where re.publicStatus == true
                              orderby re.TotalTime ascending
                              select re;
                    break;
                default: //ratings
                    SearchQ = from re in entities.Recipes
                              where re.publicStatus == true
                              orderby re.AvgRating descending
                              select re;
                    break; //TODO: Make that work properly!
            }
            if (q != null)
            {
                SearchQ = from si in SearchQ where si.Title.Contains(q) || si.Description.Contains(q) || si.Ingredients.Contains(q) select si;
            }
            if (c != null) //filter by category, if exists. if doesn't it becoms empty.
            {
                if (entities.Categories.Any(ca => ca.Name.ToLower() == c.ToLower())) //if category exists
                {
                    SearchQ = from si in SearchQ where si.Category.Name == c.ToLower() select si;
                }
                else
                {
                    SearchQ = null;
                }
            }
            if (u != null)
            {
                if (SearchQ != null)
                {
                    SearchQ = from si in SearchQ where si.User.UserName == u select si;
                }
            }
            List<SearchItem> ResultArr = new List<SearchItem>();
            if (SearchQ != null)
            {
                foreach (var item in SearchQ)
                {
                    ResultArr.Add(new SearchItem()
                    {
                        Description = item.Description,
                        Title = item.Title,
                        RouteURL = item.RouteURL,
                        image = item.MainImage,
                        Time = item.TotalTime.Value.ToString("hh\\:mm"),
                        rating = item.AvgRating,
                        Category = item.Category.Name
                    });
                }
            }
            entities.Dispose();
            return Json(new { Message = "SUCCESS", SearchResult = ResultArr }, JsonRequestBehavior.AllowGet);
        }
        #region RecipeView
        public ActionResult Recipe(string id)
        {
            if (id != null)
            {
                using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
                {
                    //find Recipe
                    try
                    {
                        ViewBag.currUserID = 0;
                        //did the user already rated?
                        var recipes = (from r in entities.Recipes where (r.RouteURL == id) select r);
                        var recipe = recipes.First();
                        if (User.Identity.IsAuthenticated)
                        {
                            int UserID = (from u in entities.Users where User.Identity.Name == u.UserName select u).First().ID;
                            ViewBag.currUserID = UserID;
                            ViewBag.UserRate = (from ra in entities.Ratings where ra.UserID == UserID && ra.Recipe_ID == recipe.ID select ra).FirstOrDefault();
                            ViewBag.SavedAlreay = false;
                            if (entities.Favorites.AsEnumerable().Any(f => f.Recipe == recipe && f.User.UserName == User.Identity.Name))
                            {
                                ViewBag.SavedAlreay = true;
                            }
                        }
                        if (recipe.publicStatus.Value)
                        {
                            //entities.Configuration.LazyLoadingEnabled = true;
                            ViewBag.ratings = recipe.Ratings;
                            ViewBag.UploadedBy = recipe.User.UserName;
                            ViewBag.IsCurrentWriter = recipe.User.UserName == User.Identity.Name;
                            float avg;
                            var ratings = ViewBag.ratings as ICollection<RecipesMVC.Models.EF_DB.Rating>;
                            try
                            {
                                avg = ratings.Sum(rating => rating.Starts) / (recipe.Ratings.Count);
                            }
                            catch (DivideByZeroException)
                            {
                                avg = 0;
                            }
                            ViewBag.avgRatings = avg.ToString();
                            // ViewBag.PageDataSchema = GenerateRecipeSchema(recipe);
                            ViewBag.Category = recipe.Category;
                            return View(recipe);
                        }
                        else
                        {
                            return new HttpUnauthorizedResult();
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        return HttpNotFound("The Recipe cannot be found. The URL my changed." +
                            "Please Contact Site Manager for solving the problem, " +
                            "if you think there has been mistake.");
                    }
                }
            }
            else
            {
                return Redirect("~/Recipes");
            }
        }
        [Authorize]
        public ActionResult Rate(RecipesMVC.Models.RateModel rate)
        {
            if ((rate.Score.HasValue || rate.CommTitle != null || rate.CommBody != null) && rate.Recipe != null)
            {
                using (Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
                {
                    try
                    {
                        Models.EF_DB.Rating NewRating;
                        var currUser = (from u in entities.Users where u.UserName == User.Identity.Name select u).First();
                        var currRecipe = from re in entities.Recipes where re.RouteURL == rate.Recipe select re;
                        if (entities.Ratings.Any(ra => ra.UserID == currUser.ID && ra.Recipe_ID == currRecipe.FirstOrDefault().ID)) //if user already responded
                        {
                            NewRating = (from ra in entities.Ratings where ra.UserID == currUser.ID && ra.Recipe_ID == currRecipe.FirstOrDefault().ID select ra).First();
                            if (rate.CommTitle != null || rate.CommBody != null)
                            {
                                NewRating.Title = rate.CommTitle;
                                NewRating.Comment = rate.CommBody;
                            }
                            else if (rate.Score.HasValue)
                            {
                                NewRating.Starts = rate.Score.Value;
                            }
                        }
                        else //if user didn't respond
                        {
                            NewRating = new Models.EF_DB.Rating()
                            {
                                Recipe_ID = currRecipe.First().ID,
                                Starts = rate.Score.Value,
                                UserID = currUser.ID,
                                UserName = User.Identity.Name
                            };
                            var ratings = entities.Set<Models.EF_DB.Rating>();
                            ratings.Add(NewRating);
                        }
                        entities.SaveChanges();
                        return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (InvalidOperationException)
                    {
                        return Json(new { Message = "Error", Error = "RecipeNotFound" });
                    }
                }
            }
            return Json(new { Message = "ERROR", Error = "SimpleInvalid" });
        }
        [NonAction]
        private string GenerateRecipeSchema(RecipesMVC.Models.EF_DB.Recipe recipe)
        {
            string ToReturn = "{";

            ToReturn += "\"@context\": \"http://schema.org\",";
            ToReturn += "\"@type\": \"Recipe\",";
            ToReturn += "\"name\": \"" + recipe.Title + "\",";
            ToReturn += "\"image\": [\"" + "CHANGE TO IMAGE UPLOAD LINK!!!" + "\"],";
            ToReturn += "\"author\": {\"@type\": \"Person\",\"name\": \"" + recipe.User.UserName + "\"},";
            ToReturn += "\"datePublished\": \"" + recipe.UploadedAt + "\",";
            ToReturn += "\"description\": \"" + recipe.Description + "\",";
            TimeSpan preptime = recipe.PrepTime.Value;
            string prepTimeString = "PT";
            if (preptime.Hours > 0)
            {
                prepTimeString += preptime.Hours.ToString() + "H";
            }

            if (preptime.Minutes > 0)
            {
                prepTimeString += preptime.Minutes.ToString() + "M";
            }

            ToReturn += "\"prepTime\": \"" + prepTimeString + "\",";

            TimeSpan cooktime = recipe.CookTime.Value;
            string cookTimeString = "PT";
            if (cooktime.Hours > 0)
            {
                cookTimeString += cooktime.Hours.ToString() + "H";
            }

            if (cooktime.Minutes > 0)
            {
                cookTimeString += cooktime.Minutes.ToString() + "M";
            }

            ToReturn += "\"cookTime\": \"" + cookTimeString + "\",";

            TimeSpan totaltime = recipe.TotalTime.Value;
            string totalTimeString = "PT";
            if (totaltime.Hours > 0)
            {
                totalTimeString += totaltime.Hours.ToString() + "H";
            }

            if (totaltime.Minutes > 0)
            {
                totalTimeString += totaltime.Minutes.ToString() + "M";
            }

            ToReturn += "\"totalTime\": \"" + totalTimeString + "\",";

            ToReturn += "\"recipeYield\": \"" + recipe.RecipeYield + " servings\",";
            ToReturn += "\"recipeCategory\": \"" + recipe.Category + "\",";
            ToReturn += "\"keywords\": \"" + recipe.KeyWords + "\",";
            // var SubQuery = (from u in entities.Users where (u.UserName == Credentials.LoginUser && u.Password == Credentials.LoginPwd) select u);

            float avg = float.Parse(ViewBag.avgRatings);
            ToReturn += "\"aggregateRating\": { \"@type\": \"AggregateRating\", \"ratingValue\": \"" + avg + "\", \"ratingCount\": \"" + recipe.Ratings.Count + "\" },";

            ToReturn += "\"RecipeIngredient\":[";
            int cntr = 0;
            foreach (string ingredient in recipe.Ingredients.Split(';'))
            {
                if (cntr != 0)
                {
                    ToReturn += ",";
                }

                ToReturn += "\"" + ingredient + "\"";
                cntr++;
            }
            ToReturn += "],";

            ToReturn += "\"RecipeInstructions\":[";
            cntr = 0;
            foreach (string instruction in recipe.Instructions.Split(';'))
            {
                if (cntr != 0)
                {
                    ToReturn += ",";
                }

                ToReturn += "{\"@type\": \"HowToStep\",";
                ToReturn += "\"text\":\"" + instruction + "\"}";
                cntr++;
            }
            ToReturn += "]";

            return ToReturn + "}";
        }
        #endregion

        #region NewRecipe
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult New()
        {
            int newRecipeIdentifier = 0;
            using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
            {
                var RecipesTable = entities.Set<RecipesMVC.Models.EF_DB.Recipe>();
                DateTime uploadDT = DateTime.Now;
                /*var lastSub = (from re in entities.Recipes orderby re.ID ascending select re);
                var last = lastSub.AsEnumerable().ToArray()[lastSub.AsEnumerable().ToArray().Count()-1];
                int NewID = last.ID += 1;*/
                var newRecipe = (new Models.EF_DB.Recipe
                {
                    UserID = (from u in entities.Users where u.UserName == User.Identity.Name select u.ID).First(),
                    publicStatus = false,
                    UploadedAt = DateTime.Now
                });
                RecipesTable.Add(newRecipe);
                entities.SaveChanges();
                var newRecipeSub = (from re in entities.Recipes orderby re.ID ascending select re);
                newRecipe = newRecipeSub.ToArray().Last();
                newRecipeIdentifier = newRecipe.ID;
            }
            return Redirect("~/Recipes/Edit/" + newRecipeIdentifier);
        }
        #endregion

        #region Edit

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities())
            {
                if (entities.Recipes.Any(re => re.ID == id))
                {
                    var RecipeToEdit = (from re in entities.Recipes where re.ID == id select re).First();
                    ViewBag.Categories = (from c in entities.Categories select c).ToArray();
                    ViewBag.AlreayCategorised = RecipeToEdit.Category != null;
                    if (RecipeToEdit.CategoryID.HasValue)
                    {
                        ViewBag.CategoryName = RecipeToEdit.Category.Name;
                    }
                    else
                    {
                        ViewBag.CategoryName = null;
                    }

                    return View(RecipeToEdit);
                }
            }
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public ActionResult Edit(RecipesMVC.Models.EF_DB.Recipe edited)
        {
            if (edited.Title != null)
            {
                if (Regex.IsMatch(edited.Title, @"^[A-Za-z\u0590-\u05fe.\-_, ]+$"))
                {
                    string RouteURLbyTitle = edited.Title.Replace(" ", String.Empty).Replace("_", String.Empty).Replace(".", String.Empty).Replace("-", String.Empty);
                    if (edited.publicStatus.HasValue)
                    {
                        if (edited.Instructions != null && edited.Ingredients != null)
                        {
                            if (edited.Ingredients.Split(';').Length > 2 && edited.Instructions.Split(';').Length > 2)
                            {
                                if (edited.RecipeYield.HasValue)
                                {
                                    if (edited.TotalTime.HasValue)
                                    {
                                        RecipesMVC.Models.EF_DB.RecipesAppEntities entities = new Models.EF_DB.RecipesAppEntities();
                                        if (entities.Recipes.Any(re => re.ID == edited.ID))
                                        {
                                            var recipe = (from re in entities.Recipes where re.ID == edited.ID select re).First();
                                            recipe.Title = edited.Title;
                                            recipe.Description = edited.Description;
                                            recipe.publicStatus = edited.publicStatus;
                                            recipe.Ingredients = edited.Ingredients;
                                            recipe.Instructions = edited.Instructions;
                                            recipe.CategoryID = edited.CategoryID;
                                            recipe.RecipeYield = edited.RecipeYield;
                                            recipe.TotalTime = edited.TotalTime;
                                            if (!(recipe.RouteURL != null))
                                            {
                                                recipe.RouteURL = RouteURLbyTitle;
                                            }
                                            //Next Version: Image Upload.
                                        }
                                        else
                                        {
                                            entities.Dispose();
                                            return Json(new { Message = "ERROR" }, JsonRequestBehavior.AllowGet);
                                        }
                                        //TODO: Add flag after entities object dispose()
                                        try
                                        {
                                            entities.SaveChanges();
                                            return Json(new { Message = "SUCCESS" }, JsonRequestBehavior.AllowGet);
                                        }
                                        catch
                                        {
                                            System.Diagnostics.Debug.WriteLine("Entity Framework DataBase error.\n at RecipeController," +
                                                "Via Edit");
                                        }
                                        finally
                                        {
                                            entities.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Json(new { Message = "ERROR" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    internal class SearchItem
    {
        public string Title { get; set; }
        public string RouteURL { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
        public Nullable<int> image { get; set; }
        public Nullable<double> rating { get; set; }
        public string Category { get; set; }
    }
}