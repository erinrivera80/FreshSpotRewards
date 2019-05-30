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
        
        public ActionResult Index()
        {
            TempData["UrlSkuGroups"] = Request.QueryString["utm_source"];
            TempData["UrlLinkSource"] = Request.QueryString["utm_medium"];
            TempData["UrlCampaign"] = Request.QueryString["utm_campaign"];
            return View();
        }

        public ActionResult Thanks()
        {
            ViewBag.Title = "Thanks for signing up for Fresh Spot Rewards!";
            return View();
        }

        public ActionResult PriorSignIn(Card card)
        {
            return View(card);
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Verify(Card card)
        {
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