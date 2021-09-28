using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CDBAAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly DevContext _devContext;

        public TicketsController(DevContext devContext)
        {
            _devContext = devContext;
        }
        // GET: api/<TicketsController>

        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> Get()
        {
            var result = _devContext.Tickets;
            return Ok(result);
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public ActionResult<Ticket> Get(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var role = claimsIdentity.FindFirst("Role").Value;

            var result = _devContext.Tickets.Where(x=>x.Id == id);
            if(result.Count()>0)
            {
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

            var email = claimsIdentity.FindFirst("Email").Value;

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
                Creator = "test",
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now
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
        public IActionResult Put(int id, [FromBody] ReturnTicket value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var userId = claimsIdentity.FindFirst("Id").Value;

            try
            {
                var updateTicket = _devContext.Tickets.Find(id);
                updateTicket.Title = value.Title;
                updateTicket.Type = value.Type;
                updateTicket.Description = value.Description;

                var ticketRecords = _devContext.TicketLogs.Where(t=>t.Id==id);

                TicketLog ticketLog = new TicketLog
                {
                    UserId = int.Parse(userId),
                    TicketId = value.Id

                };

                _devContext.SaveChanges();

                return Ok();
            }
            catch(Exception ex)
            {
                return NotFound(ex);
            }

        }

        // DELETE api/<TicketsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }

    public class ReturnTicket:Ticket
    {
        public string CodeApproval { get; set; }
        public string SALeaderApproval { get; set; }
        public string BRApproval { get; set; }
        public string DirectorApproval { get; set; }
    }

}
