namespace FreshSpotRewardsWebApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ErrorLog")]
    public partial class ErrorLog
    {
        public int ErrorLogID { get; set; }

        public DateTime? ErrorTime { get; set; }

        [StringLength(128)]
        public string ErrorSource { get; set; }

        [StringLength(2000)]
        public string ErrorMessage { get; set; }

        public int? ErrorLevel { get; set; }

        public DateTime? AlertDate { get; set; }

        public int? Status { get; set; }

        [StringLength(50)]
        public string ModuleName { get; set; }

        [StringLength(50)]
        public string ClassName { get; set; }

        [StringLength(50)]
        public string MethodName { get; set; }

        public string TargetSite { get; set; }

        public string StackTrace { get; set; }
    }
}
