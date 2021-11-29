using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketTask
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Region { get; set; }
        public string Functions { get; set; }
        public string Summary { get; set; }
        public string ReferenceNumber { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Creator { get; set; }
        public DateTime? LastModificationDateTime { get; set; }
        public string CreatorId { get; set; }
    }
}
