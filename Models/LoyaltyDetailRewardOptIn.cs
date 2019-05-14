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

        public int? ID { get; set; }

        public int? EntityCategoryID { get; set; }

        public string MobilePhone { get; set; }

        public DateTime? AddDate { get; set; }

    };
}