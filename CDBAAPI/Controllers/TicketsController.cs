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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;



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
        private readonly string _folder;

        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
                {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                {".ppt","application/vnd.ms-powerpoint" },
                {".zip", "application/zip" }
        };

        public TicketsController(DevContext devContext, IMapper mapper, IConfiguration configuration, IHostingEnvironment env)
        {
            _devContext = devContext;
            _mapper = mapper;
            Configuration = configuration;
            _folder = $@"{env.WebRootPath}\RequestSystemFiles";
        }
        
        // GET: api/<TicketsController> Get all tickets
        [HttpGet]
        public ActionResult<IEnumerable<TblTicket>> Get()
        {
            var tickets = _devContext.TblTickets.Where(x=>x.IsDeleted==false);
            var tblUser = _devContext.TblTicketUsers;
            List<TicketExtension> result = new List<TicketExtension>();
            var tasks = _devContext.TblTicketTasks.Where(x => x.IsDeleted == false);
            
            try
            {
                foreach (var ticket in tickets.ToList())
                {
                    var records = _devContext.TblTicketLogs.Where(x => x.TicketId == ticket.Id);
                    TicketExtension t = _mapper.Map<TicketExtension>(ticket);
                    if(!string.IsNullOrEmpty(ticket.Assignee))
                        t.AssigneeName = tblUser.Where(x => x.EmployeeId == ticket.Assignee).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.CreatorId))
                        t.CreatorName = tblUser.Where(x => x.EmployeeId == ticket.CreatorId).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.Developer))
                        t.DeveloperName = tblUser.Where(x => x.EmployeeId == t.Developer).FirstOrDefault().Name;
                    if(!string.IsNullOrEmpty(ticket.BusinessReviewer))
                        t.BusinessReviewerName = tblUser.Where(x => x.EmployeeId == t.BusinessReviewer).FirstOrDefault().Name;
                    if(!string.IsNullOrEmpty(ticket.SecondaryDeveloper))
                        t.SecondaryDeveloperName = tblUser.Where(x => x.EmployeeId == t.SecondaryDeveloper).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.PrimaryCodeReviewer))
                        t.PrimaryCodeReviewerName = tblUser.Where(x => x.EmployeeId == t.PrimaryCodeReviewer).FirstOrDefault().Name;
                    if (!string.IsNullOrEmpty(ticket.SecondaryCodeReviewer))
                        t.SecondaryCodeReviewerName = tblUser.Where(x => x.EmployeeId == t.SecondaryCodeReviewer).FirstOrDefault().Name;
                    
                    t.BusinessApproval = "Pending";
                    t.PrimaryCodeApproval = "Pending";
                    t.DirectorApproval = "Pending";
                    t.SALeaderApproval = "Pending";
                    t.SecondaryCodeApproval = "Pending";
                    if (records.Count() > 0)
                    {
                        foreach (TblTicketLog record in records)
                        {
                            if (!record.IsDeleted && record.ApprovalType == "businessApproval")
                            {
                                t.BusinessApproval = record.Action;
                                t.BusinessApprovalTime = record.ModificationDatetime;
                            }

                            if (!record.IsDeleted && record.ApprovalType == "primaryCodeApproval")
                            {
                                t.PrimaryCodeApproval = record.Action;
                                t.PrimaryCodeApprovalTime = record.ModificationDatetime;
                            }

                            if (!record.IsDeleted && record.ApprovalType == "secondaryCodeApproval")
                            {
                                t.SecondaryCodeApproval = record.Action;
                                t.SecondaryCodeApprovalTime = record.ModificationDatetime;
                            }

                            if (!record.IsDeleted && record.ApprovalType == "directorApproval")
                            {
                                t.DirectorApproval = record.Action;
                                t.DirectorApprovalTime = record.ModificationDatetime;
                            }

                            if (!record.IsDeleted && record.ApprovalType == "saLeaderApproval")
                            {
                                t.SALeaderApproval = record.Action;
                                t.SALeaderApprovalTime = record.ModificationDatetime;
                            }
                        }
                    }

                    if(t.TaskId!=null)
                        t.TaskName = tasks.Where(x => x.Id == t.TaskId).FirstOrDefault().TaskName;

                    result.Add(t);
                }
                return Ok(result.OrderByDescending(x=>x.Id));

            }catch(Exception ex)
            {
                return NotFound(ex);
            }           
        }


        [HttpGet("{id}")]
        public ActionResult<TicketExtension> Get(int Id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            TblTicket ticket = _devContext.TblTickets.Where(x => x.Id == Id).FirstOrDefault();

            TicketExtension result = _mapper.Map<TicketExtension>(ticket);

            var dbControl = _devContext.TblTicketDbcontrols;
            var users = _devContext.TblTicketUsers;
            List<TblTicketDbcontrol> dbControlList = _mapper.Map<List<TblTicketDbcontrol>>(dbControl);

            var creator = _devContext.TblTicketUsers.Where(x => x.EmployeeId == ticket.CreatorId.ToString()).FirstOrDefault();
            if (result != null)
            {
                result.BusinessApproval = "Pending";
                result.PrimaryCodeApproval = "Pending";
                result.DirectorApproval = "Pending";
                result.SALeaderApproval = "Pending";
                result.SecondaryCodeApproval = "Pending";
                result.DbMasterApproval = "Pending";

                var records = _devContext.TblTicketLogs.Where(x => x.TicketId == Id);
                if (records.Count() > 0)
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

                        if (!record.IsDeleted && record.ApprovalType == "dbApproval")
                        {
                            result.DbMasterApproval = record.Action;
                            result.DbMasterApprovalTime = record.ModificationDatetime;
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

        //Get all tickets under the same task
        [HttpGet("Task/{id}")]
        public ActionResult<TicketExtension> GetByTask(int Id)
        {
            var tickets = _devContext.TblTickets.Where(x => x.TaskId == Id);

            var result = _mapper.Map<List<TicketExtension>>(tickets);

            return Ok(result);
        }

        // POST api/<TicketsController> Create a new ticket
        [HttpPost]
        public IActionResult Post([FromBody] TblTicket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            if(_devContext.TblTickets.Any(x=>x.Title==value.Title))
            {
                return NotFound("This ticket name is already exist");
            }

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
                IsDeleted = false,             
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
        [HttpPut("{id}/{timeStamp}")]
        public IActionResult Put(int Id, DateTime timeStamp, [FromBody] TblTicket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var ticket = _devContext.TblTickets.Where(x => x.Id == Id).FirstOrDefault();

            if(ticket.LastModificationDateTime> timeStamp)
            {
                return NotFound("Oops. Someone has updated the ticket. Please refresh the page");
            }

            if(_devContext.TblTickets.Any(x=>x.Title==value.Title && x.Id!=value.Id))
            {
                return NotFound("This ticket name is already exist");
            }
            if (value.Status == "Reviewing")
            {
                if(!(value.Type=="Incident"||value.Type== "CYSpecialApproval") && string.IsNullOrEmpty(value.PrimaryCodeReviewer) && (ticket.Status == "UnderDevelopment"||ticket.Status == "OnHold"))
                {
                    AutoSendEmail("AssignCodeReviewer", "SALeader", ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }

                if(value.Type=="Incident" && (ticket.Status=="UnderDevelopment"||ticket.Status=="OnHold"))
                {
                    AutoSendEmail("SALeaderReminder", "SALeader", ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }

                if (ticket.Type == "CYSpecialApproval" && ticket.Status == "UnderDevelopment")
                {
                    AutoSendEmail("DirectorApproval", "Director", ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }

                if (ticket.BusinessReviewer != value.BusinessReviewer && !string.IsNullOrEmpty(value.BusinessReviewer))
                {
                    AutoSendEmail("AppointBusinessReviewer", value.BusinessReviewer, ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }

                if (ticket.PrimaryCodeReviewer != value.PrimaryCodeReviewer && !string.IsNullOrEmpty(value.PrimaryCodeReviewer))
                {
                    AutoSendEmail("AppointPrimaryCodeReviewer", value.PrimaryCodeReviewer, ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }

                if (ticket.SecondaryCodeReviewer != value.SecondaryCodeReviewer && !string.IsNullOrEmpty(value.SecondaryCodeReviewer))
                {
                    AutoSendEmail("AppointSecondaryCodeReviewer", value.SecondaryCodeReviewer, ticket.CreatorId, ticket.Assignee, ticket.Id, ticket.Title);
                }
                

            }

            if (ticket.Status=="Completed")
            {
                return NotFound("You can't modify a completed ticket");
            }

            if(ticket.Status=="Reviewing"&&(value.Status=="UnderDevelopment"||value.Status=="OnHold"))
            {
                return NotFound("You can't change a reviewing ticket back to the previous status");
            }

            if (value.Status == "Completed")
            {

                var ticketlogs = _devContext.TblTicketLogs.Where(x => x.TicketId == Id && x.IsDeleted == false && (x.Action == "Approved"||x.Action=="Completed"));

                if(value.Type == "CYSpecialApproval")
                {
                    if(ticketlogs.Any(x => x.ApprovalType == "directorApproval"))
                    {
                        UpdateTicket(Id, value);
                        return Ok();
                    }else
                    {
                        return NotFound("Can't save the ticket. It requires a director approval");
                    }
                }

                if (value.Type != "Incident" && value.BusinessReview && !ticketlogs.Any(X => X.ApprovalType == "businessApproval"))
                {
                    return NotFound("Can't save the ticket. It requires a business approval");
                }

                if (value.Type != "Incident" && !ticketlogs.Any(x => x.ApprovalType == "primaryCodeApproval"))
                {
                    return NotFound("Can't save the ticket. It requires a code review approval");
                }

                if (value.Type != "Incident" && !string.IsNullOrEmpty(value.SecondaryCodeReviewer) && !ticketlogs.Any(x => x.ApprovalType == "secondaryCodeApproval"))
                {
                    return NotFound("Can't save the ticket. It requires a secondary code review approval");
                }

                if(value.Dbmaster!=null && !ticketlogs.Any(x=>x.ApprovalType=="dbApproval"))
                {
                    return NotFound("Can't save the ticket. It requires a SA master approval");
                }

                if (value.Type == "Project" && !ticketlogs.Any(x => x.ApprovalType == "directorApproval"))
                {
                    return NotFound("Can't save the ticket. It requires CY approval");
                }

                if (!ticketlogs.Any(x => x.ApprovalType == "saLeaderApproval"))
                {
                    return NotFound("Can't save the ticket. It requires a SA leader approval");
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

            void AutoSendEmail(string emailType, string receiver, string assignee, string creator, int id, string ticketName)
            {
                var mailMessage = new MimeMessage();

                var appUrl = Configuration["AppUrl"];
                var userName ="";
                if(receiver=="SALeader")
                {
                     userName = "Kam and Wenze";
                }else if(receiver=="Director")
                {
                    userName = "CY";
                }
                else
                {
                    var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == receiver).FirstOrDefault();
                    if(user != null)
                    {
                        userName = user.Name;
                    }else
                    {
                        userName = "User";
                    }
                }
                

                switch (emailType)
                {
                    case "AssignCodeReviewer":
                        mailMessage.To.Add(new MailboxAddress("043138" + "@avnet.com"));
                        mailMessage.To.Add(new MailboxAddress("041086" + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id +"><"+ticketName+">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi Kam and Wenze,\n\n" +
                            "The following ticket is ready to be reviewed. Please assign reviewers to do the code review.\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
                    case "AppointBusinessReviewer":
                        mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi "+userName+",\n\n" +
                            "You have been appointed as the business reviewer for the ticket:\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
                    case "AppointPrimaryCodeReviewer":
                        mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress("043138" + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress("041086" + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi " + userName + ",\n\n" +
                            "You have been appointed as the primary code reviewer for the ticket:\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
                    case "AppointSecondaryCodeReviewer":
                        mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress("043138" + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress("041086" + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi " + userName + ",\n\n" +
                            "You have been appointed as the secondary code reviewer for the ticket:\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
                    case "SALeaderReminder":
                        mailMessage.To.Add(new MailboxAddress("043138" + "@avnet.com"));
                        mailMessage.To.Add(new MailboxAddress("041086" + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi Kam and Wenze,\n\n" +
                            "The following ticket is required your approval:\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
                    case "DirectorApproval":
                        mailMessage.To.Add(new MailboxAddress("904218" + "@avnet.com"));
                        mailMessage.To.Add(new MailboxAddress("902128" + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                        mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                        mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                        mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                        mailMessage.Body = new TextPart("plain")
                        {
                            Text = "Hi CY,\n\n" +
                            "The following ticket is required your approval:\n" +
                             appUrl + "Tickets/Edit/" + id +
                            "\n\nBest regards," +
                            "\nCDBA Team"
                        };
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                            smtpClient.Send(mailMessage);
                            smtpClient.Disconnect(true);
                        }
                        break;
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
                var allRecords = _devContext.TblTicketLogs.Where(x => x.TicketId == Id && x.IsDeleted == false);

                if (ticketlog.ApprovalType == "businessApproval" && ticketlog.Action == "Approved")
                {
                    if(string.IsNullOrEmpty(ticket.BusinessReviewer))
                    {
                        return NotFound("Please save the ticket before you approve the ticket");
                    }
                    AutoSendEmail("BusinessApproved", ticket.Assignee, ticket.BusinessReviewer, ticket.CreatorId, ticket.Id, ticket.Title);


                    if (string.IsNullOrEmpty(ticket.SecondaryCodeReviewer))
                    {
                        if (allRecords.Any(x => x.ApprovalType == "primaryCodeApproval" && x.Action == "Approved"))
                        {
                            AutoSendEmail("SALeaderReminder", "SALeader", ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                        }
                    }
                    else
                    {
                        if (allRecords.Any(x => x.ApprovalType == "primaryCodeApproval" && x.Action == "Approved") && allRecords.Any(x => x.ApprovalType == "secondaryCodeApproval"&& x.Action == "Approved"))
                        {
                            AutoSendEmail("SALeaderReminder", "SALeader", ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                        }
                    }
                }
                else if (ticketlog.ApprovalType == "primaryCodeApproval" && ticketlog.Action=="Approved")
                {
                    if (string.IsNullOrEmpty(ticket.PrimaryCodeReviewer))
                    {
                        return NotFound("Please save the ticket before you approve the ticket");
                    }
                    AutoSendEmail("CodeApproved", ticket.Assignee, ticket.PrimaryCodeReviewer, ticket.CreatorId, ticket.Id, ticket.Title);

                    if (string.IsNullOrEmpty(ticket.SecondaryCodeReviewer))
                    {
                        if (allRecords.Any(x => x.ApprovalType == "businessApproval" && x.Action == "Approved"))
                        {
                            AutoSendEmail("SALeaderReminder", ticket.Assignee, ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                        }
                    }
                    else
                    {
                        if (allRecords.Any(x => x.ApprovalType == "businessApproval"&&x.Action=="Approved") && allRecords.Any(x => x.ApprovalType == "secondaryCodeApproval"&&x.Action=="Approved"))
                        {
                            AutoSendEmail("SALeaderReminder", "SALeader", ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                        }
                    }
                }
                else if (ticketlog.ApprovalType == "secondaryCodeApproval" && ticketlog.Action == "Approved")
                {
                    if (string.IsNullOrEmpty(ticket.SecondaryCodeReviewer))
                    {
                        return NotFound("Please save the ticket before you approve the ticket");
                    }

                    AutoSendEmail("CodeApproved", ticket.Assignee, ticket.SecondaryCodeReviewer, ticket.CreatorId, ticket.Id, ticket.Title);

                    if (allRecords.Any(x => x.ApprovalType == "businessApproval" && x.Action == "Approved") && allRecords.Any(x => x.ApprovalType == "primaryCodeApproval" && x.Action == "Approved"))
                    {
                        AutoSendEmail("SALeaderReminder", "SALeader", ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                    }
                }
                else if (ticketlog.ApprovalType == "saLeaderApproval" && ticketlog.Action == "Approved")
                {
                    AutoSendEmail("SALeaderApproved", ticket.Assignee, ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);

                    if (ticket.Type == "Project")
                    {
                        AutoSendEmail("DirectorReminder", "Director", ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                    }
                    else
                    {   
                        if(!string.IsNullOrEmpty(ticket.Dbmaster))
                            AutoSendEmail("SAMasterReminder", ticket.Dbmaster, ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                    }
                }
                else if (ticketlog.ApprovalType == "directorApproval" && ticketlog.Action == "Approved")
                {
                    AutoSendEmail("DirectorApproved", ticket.Assignee, "Director", ticket.CreatorId, ticket.Id, ticket.Title);
                    if (!string.IsNullOrEmpty(ticket.Dbmaster))
                        AutoSendEmail("SAMasterReminder", ticket.Dbmaster, ticket.Assignee, ticket.CreatorId, ticket.Id, ticket.Title);
                }
                else if (ticketlog.ApprovalType == "dbApproval" && ticketlog.Action == "Completed")
                {
                    AutoSendEmail("SAMasterCompleted", ticket.Assignee, ticket.Dbmaster, ticket.CreatorId, ticket.Id, ticket.Title);
                }
                else
                {
                    return Ok("No Email is sent");
                }

                return Ok(ticketlog);

                 void AutoSendEmail(string emailType, string receiver, string assignee, string creator, int id, string ticketName)
                {
                    var mailMessage = new MimeMessage();

                    var appUrl = Configuration["AppUrl"];
                  
                    var userName = "";

                    if(receiver =="SALeader")
                    {
                        userName = "Kam and Wenze";
                    }
                    else if(receiver =="Director")
                    {
                        userName = "CY";
                    }
                    else
                    {
                        var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == receiver).FirstOrDefault();
                        if (user != null)
                        {
                            userName = user.Name;
                        }
                        else
                        {
                            userName = "User";
                        }
                    };

                    switch (emailType)
                    {
                        case "BusinessApproved":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi " + userName + ",\n\n" +
                                "The business review is approved for the ticket:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "CodeApproved":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi " + userName + ",\n\n" +
                                "The code review is approved for the ticket:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "SALeaderReminder":
                            mailMessage.To.Add(new MailboxAddress("043138" + "@avnet.com"));
                            mailMessage.To.Add(new MailboxAddress("041086" + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi Kam and Wenze,\n\n" +
                                "The following ticket is required your approval:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "SALeaderApproved":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress("043138" + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress("041086" + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi " + userName + ",\n\n" +
                                "The SA leader has approved the ticket:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "DirectorReminder":
                            mailMessage.To.Add(new MailboxAddress("904218" + "@avnet.com"));
                            mailMessage.To.Add(new MailboxAddress("902128" + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi CY,\n\n" +
                                "This is the reminder to approve the following ticket:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "DirectorApproved":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress("904218" + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress("902128" + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi " + userName + ",\n\n" +
                                "The director has approved the following ticket:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "SAMasterReminder":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi SA Master,\n\n" +
                                "This is a reminder for you to take actions according to the ticket requirements:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                        case "SAMasterCompleted":
                            mailMessage.To.Add(new MailboxAddress(receiver + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(creator + "@avnet.com"));
                            mailMessage.Cc.Add(new MailboxAddress(assignee + "@avnet.com"));
                            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                            mailMessage.Subject = "[CDBA LOG]<" + id + "><" + ticketName + ">";
                            mailMessage.Body = new TextPart("plain")
                            {
                                Text = "Hi " + userName + ",\n\n" +
                                "The database changes has completed:\n" +
                                 appUrl + "Tickets/Edit/" + id +
                                "\n\nBest regards," +
                                "\nCDBA Team"
                            };
                            using (var smtpClient = new SmtpClient())
                            {
                                smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                                smtpClient.Send(mailMessage);
                                smtpClient.Disconnect(true);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        [HttpGet("BusinessReviewList/{id}/{reviewType}")]
        public IActionResult BusinessReviewList(int id, string reviewType)
        {
            try
            {
                var oldBusinessReviewList = _devContext.TblTicketReviewLists.Where(x => x.TicketId == id && x.IsDeleted == false && x.ReviewType == reviewType).FirstOrDefault();
                if (oldBusinessReviewList != null)
                    return Ok(oldBusinessReviewList.Answers);
            }
            finally
            {

            }
            return Ok();
        }

        [HttpPost("BusinessReviewList/{id}/{reviewType}")]
        public IActionResult BusinessReviewList(int id, string reviewType, [FromBody] Newtonsoft.Json.Linq.JObject answers)
        {
            if (answers == null)
            {
                return NotFound();
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == employeeId).FirstOrDefault().Name;

            try
            {
                var oldBusinessReviewList = _devContext.TblTicketReviewLists.Where(x => x.TicketId == id && x.IsDeleted == false && x.ReviewType == reviewType).FirstOrDefault();

                if(oldBusinessReviewList!=null)
                {
                    oldBusinessReviewList.IsDeleted = true;
                    //_devContext.SaveChanges();
                }              
            }
            finally
            {

            }
            
            var businessReviewList = new TblTicketReviewList()
            {
                TicketId = id,
                Reviewer = user,
                ReviewerId = employeeId,
                CreatedDateTime = DateTime.Now,
                Answers = answers.ToString(),
                ReviewType = reviewType,
                IsDeleted = false
            };

            _devContext.Add(businessReviewList);
            _devContext.SaveChanges();
            return Ok();
        }

        [HttpGet("CodeReviewList/{id}/{reviewType}")]
        public IActionResult CodeReviewList(int id, string reviewType)
        {
            try
            {
                var oldCodeReviewList = _devContext.TblTicketReviewLists.Where(x => x.TicketId == id && x.IsDeleted == false && x.ReviewType == reviewType).FirstOrDefault();
                if (oldCodeReviewList != null)
                    return Ok(oldCodeReviewList.Answers);
            }
            finally
            {

            }
            return Ok();
        }

        [HttpPost("CodeReviewList/{id}/{reviewType}")]
        public IActionResult CodeReviewList(int id, string reviewType, [FromBody] Newtonsoft.Json.Linq.JObject answers)
        {
            if (answers == null)
            {
                return NotFound();
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == employeeId).FirstOrDefault().Name;

            try
            {
                var oldCodeReviewList = _devContext.TblTicketReviewLists.Where(x => x.TicketId == id && x.IsDeleted == false && x.ReviewType == reviewType).FirstOrDefault();

                if (oldCodeReviewList != null)
                {
                    oldCodeReviewList.IsDeleted = true;
                    //_devContext.SaveChanges();
                }
            }
            finally
            {

            }

            var codeReviewList = new TblTicketReviewList()
            {
                TicketId = id,
                Reviewer = user,
                ReviewerId = employeeId,
                ReviewType = reviewType,
                CreatedDateTime = DateTime.Now,
                Answers = answers.ToString(),
                IsDeleted = false
            };

            _devContext.Add(codeReviewList);
            _devContext.SaveChanges();
            return Ok();
        }

        [HttpPost("SendEmail/{id}")]//Old Send email function
        public IActionResult SendEmail(int id, TblTicket ticket)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == employeeId).FirstOrDefault();
            
            var ticketlogs = _devContext.TblTicketLogs.Where(x => x.TicketId == id && x.IsDeleted == false);

            var mailMessage = new MimeMessage();

            var appUrl = Configuration["AppUrl"];

            if (String.IsNullOrEmpty(ticket.PrimaryCodeReviewer) && ticket.Type != "Incident")
            {
                mailMessage.To.Add(new MailboxAddress("043138"+"@avnet.com"));
                mailMessage.To.Add(new MailboxAddress("041086"+"@avnet.com"));              
                mailMessage.Cc.Add(new MailboxAddress(user.EmployeeId + "@avnet.com"));
                mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                mailMessage.Subject = "Ticket Reminder";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Hi Kam and Wenze,\n\n" +
                    "The following ticket is ready to be reviewed. Please assign reviewers to do the code review.\n" +
                     appUrl + "Tickets/Edit/" + id +
                    "\n\nBest regards," +
                    "\nCDBA Team"
                };
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                return Ok();
            }

            if (!String.IsNullOrEmpty(ticket.PrimaryCodeReviewer) && !ticketlogs.Any(x=>x.ApprovalType=="primaryCodeApproval" && x.Action=="Approved"))
            {
                mailMessage.To.Add(new MailboxAddress(ticket.PrimaryCodeReviewer+"@avnet.com"));
            }

            if (!String.IsNullOrEmpty(ticket.SecondaryCodeReviewer) && !ticketlogs.Any(x => x.ApprovalType == "secondaryCodeApproval" && x.Action == "Approved"))
            {
                if(!mailMessage.To.Contains(InternetAddress.Parse(ticket.SecondaryCodeReviewer + "@avnet.com")))
                mailMessage.To.Add(new MailboxAddress(ticket.SecondaryCodeReviewer + "@avnet.com"));
            }

            if (!String.IsNullOrEmpty(ticket.BusinessReviewer) && !ticketlogs.Any(x => x.ApprovalType == "businessApproval" && x.Action == "Approved"))
            {
                if (!mailMessage.To.Contains(InternetAddress.Parse(ticket.BusinessReviewer + "@avnet.com")))
                    mailMessage.To.Add(new MailboxAddress(ticket.BusinessReviewer + "@avnet.com"));
            }

            if (ticket.Type != "Incident" && !ticketlogs.Any(x => x.ApprovalType == "saLeaderApproval" && x.Action == "Approved") && mailMessage.To == null)
            {
                //send to SA Leader
                mailMessage.To.Add(new MailboxAddress("043138"+"@avnet.com"));
                mailMessage.To.Add(new MailboxAddress("041086"+"@avnet.com"));
            }

            if (ticket.Type == "Project" && mailMessage.To.Count()==0 && !ticketlogs.Any(x => x.ApprovalType == "directorApproval" && x.Action == "Approved"))
            {
                //send to director email
                mailMessage.To.Add(new MailboxAddress("904218"+"@avnet.com"));
            }

            if(mailMessage.To.Count() == 0 && ticket.Dbmaster!=null&&!ticketlogs.Any(x=>x.ApprovalType=="dbApproval"&&x.Action=="Completed"))
            {
                mailMessage.To.Add(new MailboxAddress(ticket.Dbmaster.ToString() + "@avnet.com"));
                mailMessage.Cc.Add(new MailboxAddress(user.EmployeeId + "@avnet.com"));
                mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
                mailMessage.Subject = "Ticket Approval Reminder";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Hello SA Master,\n\n" +
                    "This is a reminder for you to take actions according to the ticket requirements:\n" +
                     appUrl + "Tickets/Edit/" + id +
                    "\n\nBest regards," +
                    "\nCDBA Team"
                };
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                return Ok();
            }

            mailMessage.Cc.Add(new MailboxAddress(user.EmployeeId + "@avnet.com"));
            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
            mailMessage.Subject = "Ticket Approval Reminder";
            mailMessage.Body = new TextPart("plain")
            {
                Text = "Hello,\n\n" +
                "This is a reminder for you to approve the following ticket:\n" +
                 appUrl + "Tickets/Edit/" + id +
                "\n\nBest regards,"+
                "\nCDBA Team"
            };

            if(mailMessage.To.Count()!=0)
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect("Smtprelay.avnet.com", 25, false);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                return Ok();
            }
            else
            {
                return NotFound("There is no approver for this ticket");
            }
            
        }
       
        [HttpPost("UploadFile/{id}")]
        public IActionResult UploadFile([FromForm] List<IFormFile> files, int id)
        {
            try
            {
                var size = files.Sum(f => f.Length);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {

                        if (!Directory.Exists($@"{_folder}\{id}"))
                        {
                            Directory.CreateDirectory($@"{_folder}\{id}");
                        }
                        var path = $@"{_folder}\{id}\{file.FileName}";
                        using (var stream = System.IO.File.Create(path))
                        {
                            file.CopyTo(stream);
                        }
                    }
                }

                return Ok("Uploaded file successfully");
            }
            catch(Exception ex)
            {
                return NotFound("Fail to upload file:" + ex);
            }

        }

        [HttpGet("DownloadFile/{id}/{fileName}")]
        [AllowAnonymous]
        public  IActionResult Download(string fileName, int id)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            var path = $@"{_folder}\{id}\{fileName}";
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                 stream.CopyTo(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            // 回傳檔案到 Client 需要附上 Content Type，否則瀏覽器會解析失敗。
            var x = _contentTypes[Path.GetExtension(path).ToLowerInvariant()];
            return File(memoryStream, x, fileName);
        }

        [HttpGet("GetFiles/{id}")]
        public IActionResult GetFiles(int id)
        {
            List<string> result = new List<string>();
            var path = $@"{_folder}\{id}";
            try
            {
                foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    result.Add(Path.GetFileName(file));
                }
                return Ok(result);

            }catch(Exception ex)
            {
                return Ok();
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
