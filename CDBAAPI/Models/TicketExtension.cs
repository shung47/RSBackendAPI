using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDBAAPI.Models
{
    public class TicketExtension: Ticket
    {
        public string DirectorApproval { get; set; }
        public string SALeaderApproval { get; set; }
        public string CodeApproval { get; set; }
        public string BusinessApproval { get; set; }
    }
}
