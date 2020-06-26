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
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (ModelState.IsValid)
            {
                if (CheckForLDROptIn(card) != 0)
                {
                    TempData["ErrorMessage"] = "You have already enrolled in Fresh Spot rewards. Thanks, you're good to go!";
                    return RedirectToAction("PriorSignIn", "Home");
                }
                CheckForHSRNumber(card);
            }
            else
            {
                TempData["ErrorMessage"] = "There was an error with sign up. Please try again.";
                RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Verify", "Home", card);
        }


        public ActionResult HotSpotAccountLookup(Card card)
        {
            string cardNo = card.AccountNumber;
            if (card.AccountNumber != null)
            {
                if (card.AccountNumber.Length == 11)
                {
                    card.AccountNumber = "62714" + cardNo;
                }
                else if (card.AccountNumber.Length == 12 )
                {
                    card.AccountNumber = "62714" + cardNo.Substring(0, 11);
                }
            }
            using (LoyayContext context = new LoyayContext())
            {
                Card oldCard = new Card();
                if (card.CH_MPHONE != null)
                {
                    oldCard = context.Cards
                        .Where(c => (c.CH_MPHONE == card.CH_MPHONE || c.CH_HPHONE == card.CH_MPHONE))
                        .Where(c => c.ProgramID == 76)
                        .Where(c => c.CH_STATUS == "P")
                        .OrderByDescending(o => o.AddDate)
                        .FirstOrDefault();
                }
                else if (card.AccountNumber != null)
                {
                    oldCard = context.Cards
                        .Where(c => c.AccountNumber == card.AccountNumber)
                        .Where(c => c.ProgramID == 76)
                        .Where(c => c.CH_STATUS == "P")
                        .OrderByDescending(o => o.AddDate)
                        .FirstOrDefault();
                }

                if (oldCard != null)
                {
                    SaveCardToSession(oldCard);
                    if (CheckForLDROptIn(card) != 0)
                    {
                        TempData["ErrorMessage"] = "You have already enrolled in Fresh Spot rewards. Thanks, you're good to go!";
                        return RedirectToAction("PriorSignIn", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Verify", "Home", oldCard);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No account was found. Try again, or sign up for a new account";
                    string url = string.Format("{0}#lookUpForm", Url.Action("Index", "Home"), 0);
                    return Redirect(url);
                }
                
            }
        }

        // check LoyaltyDetailRewardsOptIn_T_EC for MobileNumber to check for prior optin
        public int CheckForLDROptIn(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var priorOptIn = new LoyaltyDetailRewardOptIn();
                priorOptIn = context.LoyaltyDetailRewardOptIns
                    .Where(o => o.MobilePhone == card.CH_MPHONE)
                    .Where(o => o.LoyaltyDetailRewardSKUGroupID == 1017372
                    || o.LoyaltyDetailRewardSKUGroupID == 1017370
                    || o.LoyaltyDetailRewardSKUGroupID == 1017371
                    || o.LoyaltyDetailRewardSKUGroupID == 1017555
                    || o.LoyaltyDetailRewardSKUGroupID == 1017556
                    || o.LoyaltyDetailRewardSKUGroupID == 1017539
                    || o.LoyaltyDetailRewardSKUGroupID == 1017538
                    || o.LoyaltyDetailRewardSKUGroupID == 1017537
                    || o.LoyaltyDetailRewardSKUGroupID == 1017540
                    || o.LoyaltyDetailRewardSKUGroupID == 1017541)
                    .FirstOrDefault();

                if (priorOptIn == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
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
                    RedirectToAction("Verify", "Home", oldCard);
                }
                else
                {
                    CheckForLoyayCard(card);
                }
            }
        }

        // check Card table for existing Loyay card
        public void CheckForLoyayCard(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var loyayCard = new Card();
                loyayCard = context.Cards
                    .Where(c => c.CH_STATUS == "P" && c.CH_MPHONE == card.CH_MPHONE && c.ProgramID == 200000174)
                    .OrderByDescending(o => o.AddDate)
                    .FirstOrDefault();
                if (loyayCard != null)
                {
                    loyayCard.Email = card.Email;
                    SaveCardToSession(loyayCard);
                    RedirectToAction("Verify", "Home", loyayCard);
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
        public ActionResult ActivateCardEnrollment(Card card)
        {
            if (card.AccountNumber == null)
            {
                GetAcctNumberFromCardID(card);
            }
            
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
                        TempData["ErrorMessage"] = "Card could not be activated.";
                        return RedirectToAction("Error", "Home");
                    } else
                    {
                        SaveCardToSession(card);
                        return RedirectToAction("Verify", "Home");
                    }
                }
                
            }
        }

        public void SaveCardToSession(Card card)
        {
            Card cardData = new Card();
            cardData = card;
            Session["Card"] = cardData;            
        }

        public Card GetCardFromSession()
        {
            Card card = null;
            if (Session["Card"] != null)
            {
                card = Session["Card"] as Card;
            }
            else
            {
                TempData["ErrorMessage"] = "The card information did not save properly. Please try again.";
                RedirectToAction("Error", "Home");
            }
            return card;
        }

        //END - function chain for initial sign up page

        // BEGIN - function chain for Verification code chain
        // Sends verification code via text
        public ActionResult SendVerificationCode(Card cardInput)
        {
            try
            {
                var card = GetCardFromSession();

                // update new mobile number if changed
                if (card != null)
                {
                    if (cardInput.CH_MPHONE != card.CH_MPHONE)
                    {
                        card.CH_MPHONE = cardInput.CH_MPHONE;
                        UpdateCardMobileNumber(card);
                    }
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "SendMobilePhoneValidationCodeText_S_EC";

                        cmd.Parameters.Add("@CardID", SqlDbType.VarChar).Value = card.CardID;
                        cmd.Parameters.Add("@Return", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        int result;

                        conn.Open();

                        cmd.CommandTimeout = 240;
                        cmd.ExecuteNonQuery();

                        result = (int)cmd.Parameters["@Return"].Value;

                        if (result != 0)
                        {
                            TempData["ErrorMessage"] = "Something went wrong with the card activation";
                            return RedirectToAction("Error", "Home");
                        }
                        else
                        {
                            SaveCardToSession(card);
                            return RedirectToAction("Confirm", "Home");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new HomeController().LogError(ex);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
 
        }

        // Adds mobile number to Card record if different than record (at Verification Input for customer to confirm/change mobile number)
        public Card UpdateCardMobileNumber(Card card)
        {
            using (LoyayContext context = new LoyayContext())
            {
                var dbCard = context.Cards.SingleOrDefault(c => c.CardID == card.CardID);
                dbCard.CH_MPHONE = card.CH_MPHONE;
                context.SaveChanges();

                if (CheckForLDROptIn(card) == 0)
                {
                    return card;
                } else
                {
                    RedirectToAction("PriorSignIn", "Home", card);
                    return card;
                }
                
            }
        }


        // checks if mobile number verification entered by user is correct
        public ActionResult CheckVerificationNumber(Card inputCard)
        {
            var card = GetCardFromSession();
            int ret;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "CheckMobileValidationCode_S_EC";

                        cmd.Parameters.Add("@MobileNumber", SqlDbType.VarChar).Value = card.CH_MPHONE;
                        cmd.Parameters.Add("@CardID", SqlDbType.VarChar).Value = card.CardID.ToString();
                        cmd.Parameters.Add("@ValidationCode", SqlDbType.VarChar).Value = inputCard.VerificationCode;
                        cmd.Parameters.Add("@Return", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        conn.Open();

                        cmd.CommandTimeout = 240;
                        cmd.ExecuteNonQuery();

                        ret = (int)cmd.Parameters["@Return"].Value;
                    }
                }
                if (ret != 0)
                {
                    TempData["ErrorMessage"] = "Verification Number was incorrect. Please try again.";
                    return RedirectToAction("Confirm", "Home", card);
                }
                else
                {
                    EnrollFreshSpotRewards(card);
                    return RedirectToAction("Thanks", "Home");
                }
            }
            catch (Exception ex)
            {
                new HomeController().LogError(ex);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
        }


        // Enrolls in either FSR or the combo-club for Reward Spot members
        public void EnrollFreshSpotRewards(Card card)
        {
            try
            {
                LoyaltyDetailRewardOptIn optIn = new LoyaltyDetailRewardOptIn();
                if (Session["LDROptIn"] != null)
                {
                    optIn = Session["LDROptIn"] as LoyaltyDetailRewardOptIn;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "LoyaltyDetailRewardOptIn_S_EC";

                        cmd.Parameters.Add("@LoyaltyDetailRewardSKUGroupIDs", SqlDbType.VarChar).Value = optIn.LoyaltyDetailRewardSKUGroupIDs;
                        cmd.Parameters.Add("@CardID", SqlDbType.VarChar).Value = card.CardID.ToString();
                        cmd.Parameters.Add("@LinkSource", SqlDbType.VarChar).Value = "FSRWebsite";
                        cmd.Parameters.Add("@Campaign", SqlDbType.VarChar).Value = optIn.Campaign.ToString();
                        cmd.Parameters.Add("@PromoCode", SqlDbType.VarChar).Value = optIn.VendorPromoCode.ToString();

                        conn.Open();

                        cmd.CommandTimeout = 240;
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                new HomeController().LogError(ex);
            }
        }
    }
}
