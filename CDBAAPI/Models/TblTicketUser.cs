using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketUser
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
    }
}
