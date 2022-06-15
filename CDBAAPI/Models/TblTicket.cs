using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Assignee { get; set; }
        public string Developer { get; set; }
        public bool BusinessReview { get; set; }
        public bool IsRpa { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? LastModificationDateTime { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public string PrimaryCodeReviewer { get; set; }
        public string SecondaryCodeReviewer { get; set; }
        public string BusinessReviewer { get; set; }
        public int? TaskId { get; set; }
        public string SecondaryDeveloper { get; set; }
        public string Dbmaster { get; set; }
        public string CreatorId { get; set; }
        public bool? SaleaderRequired { get; set; }
        public bool? DirectorRequired { get; set; }
        public string NotificationList { get; set; }
    }
}
