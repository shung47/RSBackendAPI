using System;
using System.Collections.Generic;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class TblTicketLoginInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public string Inactive { get; set; }
        public string LoginName { get; set; }
        public string CanCreateTask { get; set; }
        public string Samaster { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public string GeneralAlert { get; set; }
    }
}
