using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using CDBAAPI.Models;

namespace CDBAAPI.AutoMapper
{
    public class TicketMapping: Profile
    {
        public TicketMapping()
        {
            CreateMap<Ticket, TicketExtension>();
        }
    }
}
