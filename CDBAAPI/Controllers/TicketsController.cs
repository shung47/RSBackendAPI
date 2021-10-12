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
        public ActionResult<IEnumerable<Ticket>> Get()
        {
            var result = _devContext.Tickets.Where(x=>x.IsDeleted==false);
            return Ok(result);
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public ActionResult<TicketExtension> Get(int Id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var role = claimsIdentity.FindFirst("Role").Value;

            Ticket ticket = _devContext.Tickets.Where(x=>x.Id == Id).First();

            TicketExtension result = _mapper.Map<TicketExtension>(ticket);

            if (result!=null)
            {
                result.BusinessApproval = "Pending";
                result.CodeApproval = "Pending";
                result.DirectorApproval = "Pending";
                result.SALeaderApproval = "Pending";

                var records = _devContext.TicketLogs.Where(x => x.TicketId == Id);
                if(records.Count()>0)
                {
                    foreach (TicketLog record in records)
                    {
                        if (!record.IsDeleted && record.ApprovalType == "businessApproval")
                        {
                            result.BusinessApproval = record.Action;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "codeApproval")
                        {
                            result.CodeApproval = record.Action;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "directorApproval")
                        {
                            result.DirectorApproval = record.Action;
                        }

                        if (!record.IsDeleted && record.ApprovalType == "saLeaderApproval")
                        {
                            result.SALeaderApproval = record.Action;
                        }
                    }
                }
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        // POST api/<TicketsController> Create a new ticket
        [HttpPost]
        public IActionResult Post([FromBody] Ticket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst("Id").Value;

            Ticket ticket = new Ticket
            {
                Title = value.Title,
                Type = value.Type,
                Description = value.Description,
                Status = "Progressing",
                Assignee = value.Assignee,
                Developer = value.Developer,
                IsRpa = value.IsRpa,
                BusinessReview = value.BusinessReview,
                CreatorId = int.Parse(userId),
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now,
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
        public IActionResult Put(int Id, [FromBody] Ticket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst("Id").Value;
            if(value.Status =="Completed")
            {

                var ticketlogs = _devContext.TicketLogs.Where(x => x.TicketId == Id && x.IsDeleted == false && x.Action =="Approved");

                if (value.BusinessReview && ticketlogs.Where(x => x.ApprovalType=="businessApproval").Count()==0)
                {
                    return NotFound("Can't save the ticket. Make sure it is approved by others.");
                }

                if (value.Type == "Project")
                {
                    if(ticketlogs.Count()==4)
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

                    return NotFound("Can't save the ticket. Make sure it is approved by others.");
                }
                else
                {
                    if (ticketlogs.Count() >= 3)
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
                    }else if(ticketlogs.Count() == 2)
                    {
                        if(value.BusinessReview)
                        {
                            UpdateTicket(Id, value);
                            return Ok();
                        }
                    }
                    return NotFound("Can't save the ticket. Make sure it is approved by others.");
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
            var updateTicket = _devContext.Tickets.Find(Id);
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

            var ticketlog = new TicketLog
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
                var records = _devContext.TicketLogs.Where(x => x.TicketId == Id && x.ApprovalType == value.ApprovalType);
                foreach(var record in records)
                {
                    record.IsDeleted = true;
                }
                _devContext.Add(ticketlog);
                _devContext.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        public void UpdateTicket(int Id, Ticket value)
        {

            var updateTicket = _devContext.Tickets.Find(Id);
            updateTicket.Title = value.Title;
            updateTicket.Type = value.Type;
            updateTicket.Description = value.Description;
            updateTicket.Status = value.Status;
            updateTicket.Assignee = value.Assignee;
            updateTicket.Developer = value.Developer;
            updateTicket.LastModificationDateTime = DateTime.Now;
            updateTicket.IsRpa = value.IsRpa;
            updateTicket.BusinessReview = value.BusinessReview;

            if(value.Status=="Completed")
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
