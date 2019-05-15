using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreshSpotRewardsWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Thanks()
        {
            ViewBag.Title = "Thanks for signing up for Fresh Spot Rewards!";

            return View();
        }

        public ActionResult PriorSignIn()
        {
            return View();
        }

        public ActionResult VerCodeIncorrect()
        {
            ViewBag.VerError = "The verification code was incorrect. Please reenter the code and try again.";
            return View();
        }

    }
}