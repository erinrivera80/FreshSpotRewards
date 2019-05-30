using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FreshSpotRewardsWebApp.Models
{
    [Table("LoyaltyDetailRewardOptIn_T_EC")]
    public partial class LoyaltyDetailRewardOptIn
    {
        public int LoyaltyDetailRewardOptInID { get; set; }

        public int? LoyaltyDetailRewardSKUGroupID { get; set; }

        [Column("ID")]
        public int? CardID { get; set; }

        public int? EntityCategoryID { get; set; }

        public string MobilePhone { get; set; }

        public DateTime? AddDate { get; set; }

        public string LinkSource { get; set; }
        
        public string Campaign { get; set; }

    };
}