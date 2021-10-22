using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using System.Security.Principal;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CDBAAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly DevContext _devContext;
        private readonly IMapper _mapper;

        public TicketsController(DevContext devContext, IMapper mapper)
        {
            _devContext = devContext;
            _mapper = mapper;
        }

        // GET: api/<TicketsController> Get all tickets
        [HttpGet]
        public ActionResult<IEnumerable<TblTicket>> Get()
        {
            var result = _devContext.TblTickets.Where(x=>x.IsDeleted==false);
            return Ok(result);
            
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public ActionResult<TicketExtension> Get(int Id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var role = claimsIdentity.FindFirst("Role").Value;

            TblTicket ticket = _devContext.TblTickets.Where(x=>x.Id == Id).First();

            TicketExtension result = _mapper.Map<TicketExtension>(ticket);

            var dbControl = _devContext.TblDbControls;

            List<TblDbControl> dbControlList = _mapper.Map<List<TblDbControl>>(dbControl);

            var creator = _devContext.TblUsers.Where(x => x.Id == ticket.CreatorId).First();
            if (result!=null)
            {
                result.BusinessApproval = "Pending";
                result.PrimaryCodeApproval = "Pending";
                result.DirectorApproval = "Pending";
                result.SALeaderApproval = "Pending";
                result.SecondaryCodeApproval = "Pending";

                var records = _devContext.TblTicketLogs.Where(x => x.TicketId == Id);
                if(records.Count()>0)
                {
                    foreach (TblTicketLog record in records)
                    {
                        if (!record.IsDeleted && record.ApprovalType == "businessApproval")
                        {
                            result.BusinessApproval = record.Action;
                            result.BusinessApprovalTime = record.ModificationDatetime;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "primaryCodeApproval")
                        {
                            result.PrimaryCodeApproval = record.Action;
                            result.PrimaryCodeApprovalTime = record.ModificationDatetime;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "secondaryCodeApproval")
                        {
                            result.SecondaryCodeApproval = record.Action;
                            result.SecondaryCodeApprovalTime = record.ModificationDatetime;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "directorApproval")
                        {
                            result.DirectorApproval = record.Action;
                            result.DirectorApprovalTime = record.ModificationDatetime;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "saLeaderApproval")
                        {
                            result.SALeaderApproval = record.Action;
                            result.SALeaderApprovalTime = record.ModificationDatetime;
                        }
                    }
                }

                result.Creator = creator.Email;
                result.DBControlList = dbControlList;
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        // POST api/<TicketsController> Create a new ticket
        [HttpPost]
        public IActionResult Post([FromBody] TblTicket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst("Id").Value;

            TblTicket ticket = new TblTicket
            {
                Title = value.Title,
                Type = value.Type,
                Description = value.Description,
                Status = "OnHold",
                Assignee = value.Assignee,
                Developer = value.Developer,
                SecondaryDeveloper = value.SecondaryDeveloper,
                IsRpa = value.IsRpa,
                BusinessReview = true,
                CreatorId = int.Parse(userId),
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now,
                TaskId = value.TaskId,
                IsDeleted = false
            };
            try
            {
                _devContext.Add(ticket);
                _devContext.SaveChanges();
                return Ok();
            }
            catch(Exception ex)
            {
                return NotFound(ex);
            }
        }

        // PUT api/<TicketsController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int Id, [FromBody] TblTicket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst("Id").Value;

            var ticket = _devContext.TblTickets.Where(x => x.Id == Id).First();
            if(ticket.Status=="Completed")
            {
                return NotFound("You can't modify a completed ticket");
            }


            if (value.Status == "Completed")
            {

                var ticketlogs = _devContext.TblTicketLogs.Where(x => x.TicketId == Id && x.IsDeleted == false && x.Action == "Approved");

                if (value.BusinessReview && !ticketlogs.Any(X => X.ApprovalType == "businessApproval"))
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by every essential peroson.");
                }
                if (value.IsRpa && !ticketlogs.Any(x => x.ApprovalType == "primaryCodeApproval"))
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by every essential peroson.");
                }

                if (value.SecondaryCodeReviewer != null && !ticketlogs.Any(x => x.ApprovalType == "secondaryCodeApproval"))
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by every essential peroson.");
                }

                if (value.Type == "Project" && !ticketlogs.Any(x => x.ApprovalType == "directorApproval"))
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by every essential peroson.");
                }

                if (value.Type != "Incident" && !ticketlogs.Any(x => x.ApprovalType == "saLeaderApproval"))
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by every essential peroson.");
                }
                try
                {
                    UpdateTicket(Id, value);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return NotFound(ex);
                }
            }
            else
            {
                try
                {
                    UpdateTicket(Id, value);
                    return Ok();
                }
                catch (Exception ex)
                {
                    return NotFound(ex);
                }
            }
        }

        // DELETE api/<TicketsController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int Id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst("Id").Value;
            var updateTicket = _devContext.TblTickets.Find(Id);
            if (int.Parse(userId) == updateTicket.CreatorId)
            {
                updateTicket.IsDeleted = true;
                _devContext.SaveChanges();
                return Ok();
            }
            else
            {
                return NotFound("Sorry, you don't have permission to delete this ticket");
            }

        }

        //Approve ticket
        [HttpPost("{Id}")]
        public IActionResult Post(int Id, TicketApproval value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var email = claimsIdentity.FindFirst("Email").Value;

            var ticket = _devContext.TblTickets.Where(x => x.Id == Id).First();
            if(ticket.Status=="Completed")
            {
                return NotFound("You can't modify a completed ticket");
            }

            var ticketlog = new TblTicketLog
            {
                UserId = int.Parse(claimsIdentity.FindFirst("Id").Value),
                TicketId = Id,
                Action = value.ApprovalStatus,
                ModificationDatetime = DateTime.Now,
                IsDeleted = false,
                ApprovalType = value.ApprovalType
            };

            try
            {
                var records = _devContext.TblTicketLogs.Where(x => x.TicketId == Id && x.ApprovalType == value.ApprovalType);
                foreach(var record in records)
                {
                    record.IsDeleted = true;
                }
                _devContext.Add(ticketlog);
                _devContext.SaveChanges();

                return Ok(ticketlog);
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        public void UpdateTicket(int Id, TblTicket value)
        {

            var updateTicket = _devContext.TblTickets.Find(Id);
            updateTicket.Title = value.Title;
            updateTicket.Type = value.Type;
            updateTicket.Description = value.Description;
            updateTicket.Status = value.Status;
            updateTicket.Assignee = value.Assignee;
            updateTicket.Developer = value.Developer;
            updateTicket.SecondaryDeveloper = value.SecondaryDeveloper;
            updateTicket.LastModificationDateTime = DateTime.Now;
            updateTicket.IsRpa = value.IsRpa;
            updateTicket.BusinessReview = value.BusinessReview;
            updateTicket.BusinessReviewer = value.BusinessReviewer;
            updateTicket.PrimaryCodeReviewer = value.PrimaryCodeReviewer;
            updateTicket.SecondaryCodeReviewer = value.SecondaryCodeReviewer;
            updateTicket.TaskId = value.TaskId;
            updateTicket.Dbmaster = value.Dbmaster;

            if (value.Status=="Completed")
            {
                updateTicket.CompletedDateTime = DateTime.Now;
            }

            _devContext.SaveChanges();
            
        }

    }



    public class TicketApproval
    {
        public string ApprovalType { get; set; }
        public string ApprovalStatus { get; set; }
    }

}
