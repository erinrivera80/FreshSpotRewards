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
        LoyayContext db = new LoyayContext();

        public ActionResult Index()
        {
            LoyaltyDetailRewardOptIn optInRecord = new LoyaltyDetailRewardOptIn();
            
            optInRecord.LoyaltyDetailRewardSKUGroupIDs = Request.QueryString["utm_source"];
            optInRecord.LinkSource = Request.QueryString["utm_medium"];
            optInRecord.Campaign = Request.QueryString["utm_campaign"];
            optInRecord.VendorPromoCode = Request.QueryString["promoCode"];

            if (optInRecord.LoyaltyDetailRewardSKUGroupIDs == null)
            {
                optInRecord.LoyaltyDetailRewardSKUGroupIDs = "1017372,1017370,1017371,1017555,1017556,1017539,1017538,1017537,1017540,1017541";
            }
            if (optInRecord.Campaign == null)
            {
                optInRecord.Campaign = "none";
            }
            if (optInRecord.VendorPromoCode == null)
            {
                optInRecord.VendorPromoCode = "none";
            }

            Session["LDROptIn"] = optInRecord;
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

        public void LogError(Exception ex)
        {
            using (db = new LoyayContext())
            {
                ErrorLog log = new ErrorLog()
                {
                    ErrorTime = DateTime.Now,
                    ErrorSource = "Card Holder Website",
                    ErrorMessage = ex.Message,
                    ModuleName = ex.Source,
                    TargetSite = ex.TargetSite.ToString(),
                    StackTrace = ex.StackTrace
                };

                db.ErrorLog.Add(log);
                db.SaveChanges();
            }

        }

    }
}