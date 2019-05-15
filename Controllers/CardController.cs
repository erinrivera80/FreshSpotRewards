using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FreshSpotRewardsWebApp.Models;

namespace FreshSpotRewardsWebApp.Controllers
{
    public class CardController : Controller
    {
        private readonly string skuGroups = "1017353,1017368,1017369,1017371,1017370,1017372";

        // GET: Card/Create
        public ActionResult Create()
        {
            Card card = new Card();
            return View(card);
        }

        // POST: Card/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Card card)
        {
            if (ModelState.IsValid)
            {
                CheckVerificationNumber(card);
                return RedirectToAction("Thanks", "Home");
            }
            return RedirectToAction("Thanks", "Home");
        }

        // check LoyaltyDetailRewardsOptIn_T_EC for MobileNumber to check for prior optin
        public void CheckForLDROptIn(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var priorOptIn = new LoyaltyDetailRewardOptIn();
                priorOptIn = context.LoyaltyDetailRewardOptIns
                    .Where(o => o.MobilePhone == card.CH_MPHONE && o.LoyaltyDetailRewardSKUGroupID == 1017353)
                    .FirstOrDefault();

                if (priorOptIn != null)
                {
                    RedirectToAction("PriorOptIn", "Home");
                }
                else
                {
                    CheckForHSRNumber(card);
                }
            }
        }

        // checks if mobile number verification entered by user is correct
        public void CheckVerificationNumber(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                SqlParameter mobNum = new SqlParameter("@MobileNumber", card.CH_MPHONE)
                {
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter cardID = new SqlParameter("@CardID", card.CardID)
                {
                    SqlDbType = SqlDbType.Int
                };
                SqlParameter validCode = new SqlParameter("@ValidationCode", card.VerificationCode)
                {
                    SqlDbType = SqlDbType.VarChar
                };

                var result = context.Database.ExecuteSqlCommand("CheckMobileValidationCode_S_EC @MobileNumber, @CardID, @ValidationCode",
                    mobNum, cardID, validCode);
                
                if (result == -1)
                {
                    RedirectToAction("VerCodeIncorrect", "Home");
                }
                else
                {
                    CheckForLDROptIn(card);
                }
            }
        }

        // check Card table for HotSpotRewards card
        public void CheckForHSRNumber(Card card)
        { 
            using (LoyayContext context = new LoyayContext())
            {
                var oldCard = new Card();
                oldCard = context.Cards
                    .Where(c => c.CH_STATUS == "P" && c.CH_MPHONE == card.CH_MPHONE && c.ProgramID == 76)
                    .OrderByDescending(o => o.AddDate)
                    .FirstOrDefault();
                if (oldCard != null)
                {
                    EnrollFreshSpotRewards(oldCard);
                }
                else
                {
                    GetNextCard(card);
                }
            }
        }

        // Get next unassigned Loyay card
        public void GetNextCard(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                SqlParameter prog = new SqlParameter("@ProgramID", 200000174)
                {
                    SqlDbType = SqlDbType.Int
                };
                SqlParameter cat = new SqlParameter("@CategoryID", 1823)
                {
                    SqlDbType = SqlDbType.Int
                };
                var cardID = new SqlParameter("@CardID", SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                context.Database.ExecuteSqlCommand(
                    "GetNextAvailableCardIDByLocationCategory_S_EC @ProgramID, @CategoryID, @CardID out",
                    prog, cat, cardID);

                card.CardID = (int)cardID.Value;
            }

            UpdateCardData(card);
            EnrollFreshSpotRewards(card);
        }

        // Adds email and mobile number to new Card record
        public void UpdateCardData(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var dbCard = context.Cards.SingleOrDefault(c => c.CardID == card.CardID);
                if (dbCard != null)
                {
                    dbCard.CH_MPHONE = card.CH_MPHONE;
                    dbCard.Email = card.Email;
                    context.SaveChanges();
                }
            }
        }

        // Enrolls non-Hot Spot members in the Fresh Spot Rewards club
        public void EnrollFreshSpotRewards(Card card)
        {
            using (var context = new LoyayContext())
            {
                // TO-DO: Add mobile number parameter
                SqlParameter skus = new SqlParameter("@SkuGroups", skuGroups);
                SqlParameter cardID = new SqlParameter("@CardID", card.CardID);
                var query = context.Database.ExecuteSqlCommand("LoyaltyDetailRewardOptIn_S_EC @SkuGroups, @CardID", skus, cardID);
            }

            RedirectToAction("Thanks", "Home");
        }
    }
}
