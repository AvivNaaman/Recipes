using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;

namespace RecipesMVC.Models
{
    public class ConfigurationModel
    {
        public bool SmtpEnabled { get; set; }
        public SmtpModel Smtp { get; set; }
        public string FontFamily { get; set; }
        public string SiteName { get; set; }
        public bool DarkMode { get; set; }
        public ConfigurationModel(bool readConfig)
        {
            if (readConfig)
            {
                SmtpEnabled = bool.Parse(ConfigurationManager.AppSettings["SmtpEnabled"]);
                if (SmtpEnabled)
                {
                    Smtp = new SmtpModel();
                    Smtp.User = ConfigurationManager.AppSettings["SmtpUser"];
                    Smtp.Password = ConfigurationManager.AppSettings["SmtpPassword"];
                    Smtp.Server = ConfigurationManager.AppSettings["SmtpServer"];
                }
                FontFamily = ConfigurationManager.AppSettings["FontFamily"];
                SiteName = ConfigurationManager.AppSettings["SiteName"];
                DarkMode = bool.Parse(ConfigurationManager.AppSettings["DarkMode"]);
            }
        }
        public ConfigurationModel()
        {

        }
    }
    public class SmtpModel
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
    }
}