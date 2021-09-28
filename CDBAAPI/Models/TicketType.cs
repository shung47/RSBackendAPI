using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TicketType
    {
        public int Id { get; set; }
        public string TypeTitle { get; set; }
        public bool DirectorApproval { get; set; }
        public bool SaleaderApproval { get; set; }
        public bool BusinessReview { get; set; }
        public bool CodeReview { get; set; }
    }
}
