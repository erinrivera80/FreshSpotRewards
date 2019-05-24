using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Mvc;
using FreshSpotRewardsWebApp.Models;

namespace FreshSpotRewardsWebApp.Controllers
{
    public class CardController : Controller
    {
        private readonly string skuGroups = "1017353,1017368,1017369,1017371,1017370,1017372";

        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LoyayContext"].ConnectionString;

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
                CheckForLDROptIn(card);
            }
            else
            {
                RedirectToAction("Error", "Home", new { errorMessage = "There was an error with sign up. Please try again." });
            }

            return RedirectToAction("Verify", "Home", card);
        }

        // check LoyaltyDetailRewardsOptIn_T_EC for MobileNumber to check for prior optin
        public void CheckForLDROptIn(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var priorOptIn = new LoyaltyDetailRewardOptIn();
                priorOptIn = context.LoyaltyDetailRewardOptIns
                    .Where(o => o.MobilePhone == card.CH_MPHONE
                        && o.LoyaltyDetailRewardSKUGroupID == 1017353)
                        //|| o.LoyaltyDetailRewardSKUGroupID == 1017370
                        //|| o.LoyaltyDetailRewardSKUGroupID == 1017371
                        //|| o.LoyaltyDetailRewardSKUGroupID == 1017372)
                    .FirstOrDefault();

                if (priorOptIn != null)
                {
                    RedirectToAction("PriorOptIn", "Home", card);
                }
                else
                {
                    CheckForHSRNumber(card);
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
                    oldCard.Email = card.Email;
                    SaveCardToSession(oldCard);
                    EnrollFreshSpotRewards(oldCard);
                }
                else
                {
                    GetNextCard(card);
                }
            }
        }

        public Card HotSpotAccountLookup(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                Card oldCard = new Card();
                if (card.Email != null || card.Email != "")
                {
                    oldCard = context.Cards
                        .Where(c => c.Email == card.Email)
                        .Where(c => c.ProgramID == 76)
                        .OrderByDescending(o => o.AddDate)
                        .FirstOrDefault();
                }
                else if (card.CH_MPHONE != null || card.CH_MPHONE != "")
                {
                    oldCard = context.Cards
                        .Where(c => c.CH_MPHONE == card.CH_MPHONE)
                        .Where(c => c.ProgramID == 76)
                        .OrderByDescending(o => o.AddDate)
                        .FirstOrDefault();
                }
                else if (card.AccountNumber != null || card.AccountNumber != "")
                {
                    oldCard = context.Cards
                        .Where(c => c.AccountNumber == card.AccountNumber)
                        .Where(c => c.ProgramID == 76)
                        .OrderByDescending(o => o.AddDate)
                        .FirstOrDefault();
                }
               
                return oldCard;
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

            ActivateCardEnrollment(card);
        }

        public Card GetAcctNumberFromCardID(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var dbCard = context.Cards.Where(x => x.CardID == card.CardID).FirstOrDefault();
                card.AccountNumber = dbCard.AccountNumber;
                return card;
            }
        }

        // Runs stored proc to activate card enrollment
        public void ActivateCardEnrollment(Card card)
        {
            GetAcctNumberFromCardID(card);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "CardholderEnrollmentRequest_S_EC";

                    cmd.Parameters.Add("@IssuerID", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_FNAME", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_LNAME", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HADDR1", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HADDR2", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HCITY", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HSTATE", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@Country", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HZIP", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_HPHONE", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_WPHONE", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@phonefax", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = card.Email;
                    cmd.Parameters.Add("@CH_SSN", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_BNKACCT", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@CH_ABA", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@BankName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@BankCity", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@UserDefine1", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@UserDefine2", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@UserDefine3", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@UserDefine4", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@UserDefine5", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@AccountNumber", SqlDbType.VarChar).Value = card.AccountNumber;
                    cmd.Parameters.Add("@MobileNumber", SqlDbType.VarChar).Value = card.CH_MPHONE;
                    cmd.Parameters.Add("@Return", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int activateID;

                    conn.Open();

                    cmd.CommandTimeout = 240;
                    cmd.ExecuteNonQuery();

                    activateID = (int)cmd.Parameters["@Return"].Value;


                    if (activateID < 10)
                    {
                        RedirectToAction("Error", "Home", new { errorMessage = "Something went wrong with card activation. Please try again later." });
                    } else
                    {
                        EnrollFreshSpotRewards(card);
                    }
                }
                
            }
        }

        // Enrolls in either FSR or the combo-club for Reward Spot members
        public ActionResult EnrollFreshSpotRewards(Card card)
        {
            using (var context = new LoyayContext())
            {
                SqlParameter skus = new SqlParameter("@SkuGroups", skuGroups);
                SqlParameter cardID = new SqlParameter("@CardID", card.CardID);
                var query = context.Database.ExecuteSqlCommand("LoyaltyDetailRewardOptIn_S_EC @SkuGroups, @CardID", skus, cardID);
                SaveCardToSession(card);

                return View("Verify", "Home", card);
            };
        }

        public void SaveCardToSession(Card card)
        {
            Card cardData = new Card();
            cardData = card;
            Session["Card"] = cardData;            
        }

        //END - function chain for initial sign up page

        // BEGIN - function chain for Verification code chain
        // Sends verification code via text
        public ActionResult SendVerificationCode(Card cardInput)
        {
            // pull Card from session
            Card card = null;
            if (Session["Card"] != null)
            {
                card = Session["Card"] as Card;
            }
            else
            {
                RedirectToAction("Error", "Home", new { errorMessage = "The card information did not save properly. Please try again." });
            }

            // update new mobile number if changed
            // TO DO - Need to verify number change not HSR member/LDR opt in
            if (cardInput.CH_MPHONE != card.CH_MPHONE)
            {
                card.CH_MPHONE = cardInput.CH_MPHONE;
                UpdateCardMobileNumber(card);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SendMobilePhoneValidationCodeText_S_EC";

                    cmd.Parameters.Add("@CardID", SqlDbType.VarChar).Value = 206700750;
                    cmd.Parameters.Add("@Return", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int result;

                    conn.Open();

                    cmd.CommandTimeout = 240;
                    cmd.ExecuteNonQuery();

                    result = (int)cmd.Parameters["@Return"].Value;

                    if (result != 0)
                    {
                            return RedirectToAction("Error", "Home", new { errorMessage = "Something went wrong with card activation. Please try again later." });
                    } else
                    {
                        return RedirectToAction("Confirm", "Home", card);
                    }
                }
            }
 
        }

        // Adds mobile number to Card record if different than record (at Verification Input for customer to confirm/change mobile number)
        public void UpdateCardMobileNumber(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var dbCard = context.Cards.SingleOrDefault(c => c.CardID == card.CardID);
                if (dbCard != null)
                {
                    dbCard.CH_MPHONE = card.CH_MPHONE;
                    context.SaveChanges();
                }
            }
        }


        // checks if mobile number verification entered by user is correct
        public ActionResult CheckVerificationNumber(Card cardInput)
        {
            Card card = new Card
            {
                CardID = 206700750,
                CH_MPHONE = "5027597903",
                VerificationCode = cardInput.VerificationCode
            };

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "CheckMobileValidationCode_S_EC";

                    cmd.Parameters.Add("@MobileNumber", SqlDbType.VarChar).Value = card.CH_MPHONE;
                    cmd.Parameters.Add("@CardID", SqlDbType.VarChar).Value = card.CardID;
                    cmd.Parameters.Add("@ValidationCode", SqlDbType.VarChar).Value = card.VerificationCode;
                    cmd.Parameters.Add("@Return", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    int result;

                    conn.Open();

                    cmd.CommandTimeout = 240;
                    cmd.ExecuteNonQuery();

                    result = (int)cmd.Parameters["@Return"].Value;

                    if (result != 0)
                    {
                        return RedirectToAction("Error", "Home", new { errorMessage = "Verification code was incorrect. Please try again." });
                    }
                    else
                    {
                        return RedirectToAction("Thanks", "Home");
                    }
                }
            }
        }
    }
}
