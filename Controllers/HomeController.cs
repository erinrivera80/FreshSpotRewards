using FreshSpotRewardsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreshSpotRewardsWebApp.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index(string skuGroups)
        {
            ViewBag.SkuGroups = skuGroups;
            return View();
        }

        public ActionResult CardIndex(Card card)
        {
            ViewBag.MobNum = card.CH_MPHONE;
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

        public ActionResult Error(TempDataDictionary keyValuePairs)
        {
            TempData["ErrorMessage"] = TempData["ErrorMessage"];
            return View();
        }

        public ActionResult Verify(Card card)
        {
            ViewBag.MobileNumber = card.CH_MPHONE;
            return View(card);
        }

        public ActionResult Confirm(Card card)
        {
            return View();
        }

        public ActionResult SignUp(Card card)
        {
            return View(card);
        }

    }
}