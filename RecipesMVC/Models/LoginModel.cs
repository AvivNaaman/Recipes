using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RecipesMVC.Models
{
    //<summary>
    //this model is being used as login credentails
    //model only.
    public class LoginModel
    {
        public string LoginUser { get; set; }
        public string LoginPwd { get; set; }
        public bool RememberMe { get; set; }
        public LoginModel()
        {

        }
    }
}