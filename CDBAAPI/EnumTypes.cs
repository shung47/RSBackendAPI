using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDBAAPI
{
    public class EnumTypes
    {
        public enum ApprovalStatus
        {
            Pending,
            Approved,
            Rejected
        };

        public enum ApprovalTypes
        {
            primaryCodeApproval,
            secondaryCodeApproval,
            businessApproval,
            directorApproval,
            saLeaderApproval,
            dbApproval
        }

        public enum TicketStatus
        {
            OnHold,
            UnderDevelopment,
            Reviewing,
            Completed,
            Cancelled
        };

        public enum TicketTypes
        {
            Project,
            SR,
            SSR,
            Incident,
            CYSpecialApproval
        };
    }
}
