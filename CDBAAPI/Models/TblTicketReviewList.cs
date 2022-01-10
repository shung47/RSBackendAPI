using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketReviewList
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Reviewer { get; set; }
        public string ReviewerId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Answers { get; set; }
        public bool IsDeleted { get; set; }
        public string ReviewType { get; set; }
    }
}
