using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Action { get; set; }
        public DateTime ModificationDatetime { get; set; }
        public bool IsDeleted { get; set; }
        public string ApprovalType { get; set; }
        public string EmployeeId { get; set; }
    }
}
