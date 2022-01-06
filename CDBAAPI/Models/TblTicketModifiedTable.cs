using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketModifiedTable
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? LastModificationDateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
