using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string CommentContent { get; set; }
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public DateTime? LastModificationDateTime { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
