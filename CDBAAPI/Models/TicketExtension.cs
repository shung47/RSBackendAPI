﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDBAAPI.Models
{
    public class TicketExtension: TblTicket
    {
        public string Creator { get; set; }
        public string DirectorApproval { get; set; }
        public string SALeaderApproval { get; set; }
        public string PrimaryCodeApproval { get; set; }
        public string SecondaryCodeApproval { get; set; }
        public string BusinessApproval { get; set; }
        public DateTime DirectorApprovalTime { get; set; }
        public DateTime SALeaderApprovalTime { get; set; }
        public DateTime PrimaryCodeApprovalTime { get; set; }
        public DateTime SecondaryCodeApprovalTime { get; set; }
        public DateTime BusinessApprovalTime { get; set; }
    }
}
