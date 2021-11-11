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
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;



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
        private readonly IConfiguration Configuration;

        public TicketsController(DevContext devContext, IMapper mapper, IConfiguration configuration)
        {
            _devContext = devContext;
            _mapper = mapper;
            Configuration = configuration;
        }

        // GET: api/<TicketsController> Get all tickets
        [HttpGet]
        public ActionResult<IEnumerable<TblTicket>> Get()
        {
            var tickets = _devContext.TblTickets.Where(x=>x.IsDeleted==false);
            var tblUser = _devContext.TblUsers;
            List<TicketExtension> result = new List<TicketExtension>();

            try
            {
                foreach (var ticket in tickets.ToList())
                {
                    TicketExtension t = _mapper.Map<TicketExtension>(ticket);
                    if(!string.IsNullOrEmpty(ticket.Assignee))
                        t.AssigneeName = tblUser.Where(x => x.EmployeeId == ticket.Assignee).FirstOrDefault().Name;
                    if(!string.IsNullOrEmpty(ticket.Developer))
                        t.DeveloperName = tblUser.Where(x => x.EmployeeId == t.Developer).FirstOrDefault().Name;
                    if(!string.IsNullOrEmpty(ticket.BusinessReviewer))
                        t.BusinessReviewerName = tblUser.Where(x => x.EmployeeId == t.BusinessReviewer).FirstOrDefault().Name;
                    if(!string.IsNullOrEmpty(ticket.SecondaryDeveloper))
                        t.SecondaryDeveloperName = tblUser.Where(x => x.EmployeeId == t.SecondaryDeveloper).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.PrimaryCodeReviewer))
                        t.PrimaryCodeReviewerName = tblUser.Where(x => x.EmployeeId == t.PrimaryCodeReviewer).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.SecondaryCodeReviewer))
                        t.SecondaryCodeReviewerName = tblUser.Where(x => x.EmployeeId == t.SecondaryCodeReviewer).FirstOrDefault().Name;
                    result.Add(t);
                }
                return Ok(result);

            }catch(Exception ex)
            {
                return NotFound(ex);
            }           
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public ActionResult<TicketExtension> Get(int Id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            TblTicket ticket = _devContext.TblTickets.Where(x=>x.Id == Id).FirstOrDefault();

            TicketExtension result = _mapper.Map<TicketExtension>(ticket);

            var dbControl = _devContext.TblDbControls;

            List<TblDbControl> dbControlList = _mapper.Map<List<TblDbControl>>(dbControl);

            var creator = _devContext.TblUsers.Where(x => x.EmployeeId == ticket.CreatorId.ToString()).FirstOrDefault();
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

                result.Creator = creator.EmployeeId;
                result.CreatorName = creator.Name;
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

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

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
                CreatorId = employeeId,
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

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var ticket = _devContext.TblTickets.Where(x => x.Id == Id).FirstOrDefault();
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
            var EmployeeId = claimsIdentity.FindFirst("EmployeeId").Value;
            var updateTicket = _devContext.TblTickets.Find(Id);
            if (EmployeeId == updateTicket.CreatorId && updateTicket.Status!="Completed")
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

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var ticket = _devContext.TblTickets.Where(x => x.Id == Id).FirstOrDefault();
            if(ticket.Status=="Completed")
            {
                return NotFound("You can't modify a completed ticket");
            }

            var ticketlog = new TblTicketLog
            {
                EmployeeId = claimsIdentity.FindFirst("EmployeeId").Value,
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

        [HttpGet("SendEmail/{id}")]
        public IActionResult SendEmail(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var user = _devContext.TblUsers.Where(x => x.EmployeeId == employeeId).FirstOrDefault();

            var ticket = _devContext.TblTickets.Where(x => x.Id == id).FirstOrDefault();
            
            var ticketlogs = _devContext.TblTicketLogs.Where(x => x.TicketId == id && x.IsDeleted == false);

            var mailMessage = new MimeMessage();

            if (ticket.PrimaryCodeReviewer!=null&& !ticketlogs.Any(x=>x.ApprovalType=="primaryCodeApproval" && x.Action=="Approved"))
            {
                mailMessage.To.Add(new MailboxAddress(user.Name, user.EmployeeId+"@avnet.com"));
            }

            if (ticket.SecondaryCodeReviewer != null && !ticketlogs.Any(x => x.ApprovalType == "secondaryCodeApproval" && x.Action == "Approved"))
            {
                if(!mailMessage.To.Contains(InternetAddress.Parse(user.EmployeeId + "@avnet.com")))
                mailMessage.To.Add(new MailboxAddress(user.Name, user.EmployeeId + "@avnet.com"));
            }

            if (ticket.BusinessReviewer != null && !ticketlogs.Any(x => x.ApprovalType == "businessApproval" && x.Action == "Approved"))
            {
                if (!mailMessage.To.Contains(InternetAddress.Parse(user.EmployeeId + "@avnet.com")))
                    mailMessage.To.Add(new MailboxAddress(user.Name, user.EmployeeId + "@avnet.com"));
            }

            if (ticket.Type != "Incident" && !ticketlogs.Any(x => x.ApprovalType == "saLeaderApproval" && x.Action == "Approved"))
            {
                //send to SA Leader
                //mailMessage.To.Add(new MailboxAddress("", ""));
            }

            if (ticket.Type == "Project" && mailMessage.To==null && !ticketlogs.Any(x => x.ApprovalType == "directorApproval" && x.Action == "Approved"))
            {
                //send to director email
                //mailMessage.To.Add(new MailboxAddress("", ""));
            }
            var appUrl = Configuration["AppUrl"];
            mailMessage.From.Add(new MailboxAddress("CDBA_AUTO", "CDBA-AUTO@AVNET.COM"));
            mailMessage.Subject = "Approval Reminder";
            mailMessage.Body = new TextPart("plain")
            {
                Text = "Hello,\n\n" +
                "This is a reminder for you to approve the following ticket:\n" +
                 appUrl + "Tickets/Edit/" + id +
                "\n\n Best regards,"+
                "\nCDBA Team"
            };

            if(mailMessage.To.Count()!=0)
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                    //smtpClient.Authenticate("user", "password");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
            }
            return Ok();
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
