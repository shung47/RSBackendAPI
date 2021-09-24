using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // POST api/<TicketsController>
        [HttpPost]
        public IActionResult Post([FromBody] Ticket value)
        {
            Ticket ticket = new Ticket
            {
                Title = value.Title,
                Type = value.Type,
                Description = value.Description,
                Status = "Unassign"
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
        public IActionResult Put(int id, [FromBody] Ticket value)
        {
            try
            {
                var updateTicket = _devContext.Tickets.Find(id);
                updateTicket.Title = value.Title;
                updateTicket.Type = value.Type;
                updateTicket.Description = value.Description;

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
}
