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

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(13)]
        public string CH_MPHONE { get; set; }

        public DateTime? AddDate { get; set; }

        [Column("RESERVED1")]
        public string VerificationCode { get; set; }

        public string AccountNumber { get; set; }
    };

}
