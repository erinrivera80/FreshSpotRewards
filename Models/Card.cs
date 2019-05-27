using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FreshSpotRewardsWebApp.Models
{ 
    [Table("Card")]
    public partial class Card
    {
        public int CardID { get; set; }

        public int? EntityID { get; set; }

        public int? EntityCategoryID { get; set; }

        public int? ProgramID { get; set; }
        
        public int? MerchantID { get; set; }

        public int? LocationID { get; set; }

        [StringLength(6)]
        public string M_IDNUM { get; set; }

        [StringLength(1)]
        public string CH_STATUS { get; set; }
        
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")]
        [StringLength(50)]
        public string Email { get; set; }

        [RegularExpression("([0-9]+)")]
        [StringLength(13, ErrorMessage = "Mobile number must be 10 digits", MinimumLength = 10)]
        public string CH_MPHONE { get; set; }

        public DateTime? AddDate { get; set; }

        [RegularExpression("([0-9]+)")]
        [StringLength(6, ErrorMessage = "Verification code requires 6 digits", MinimumLength = 6)]
        [Column("RESERVED1")]
        public string VerificationCode { get; set; }

        [Column("RESERVED2")]
        public string SkuGroupIds { get; set; }

        public string AccountNumber { get; set; }
    };
}
