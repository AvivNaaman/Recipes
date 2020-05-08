using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipesMVC.Models
{
    public class RateModel
    {
        public Nullable<int> Score { get; set; }
        public string Recipe { get; set; }
        public string CommTitle { get; set; }
        public string CommBody { get; set; }
    }
}