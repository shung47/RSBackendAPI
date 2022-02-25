using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
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
    public class TicketModifiedTablesController : ControllerBase
    {
        private readonly DevContext _devContext;
        public TicketModifiedTablesController(DevContext devContext)
        {
            _devContext = devContext;
        }

        // GET api/<TicketModifiedTablesController>/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<TblTicketModifiedTable>> Get(int id)
        {
            try
            {
                var tables = _devContext.TblTicketModifiedTables.Where(x => x.TicketId == id && x.IsDeleted==false);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // POST api/<TicketModifiedTablesController>
        [HttpPost]
        public IActionResult Post([FromBody] TblTicketModifiedTable value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var employee = claimsIdentity.FindFirst("Name").Value;

            var tables = _devContext.TblTicketModifiedTables.Where(x => x.TicketId == value.TicketId && x.IsDeleted==false);

            var ticket = _devContext.TblTickets.Where(x => x.Id == value.TicketId).FirstOrDefault();

            if(ticket!=null)
            {
                if(ticket.Status=="Completed")
                {
                    return NotFound("Can't add the object for a completed ticket");
                }
            }
            foreach(var table in tables)
            {
                if (table.DatabaseName != value.DatabaseName)
                {
                    return NotFound("Can't add the object in different database");
                }
            }
            TblTicketModifiedTable tblTicketModifiedTable = new TblTicketModifiedTable
            {
                TicketId = value.TicketId,
                DatabaseName = value.DatabaseName,
                TableName = value.TableName,
                Creator = employee,
                CreatorId = employeeId,
                LastModificationDateTime = DateTime.Now,
                CreateDateTime = DateTime.Now,
                IsDeleted = false,
                Summary = value.Summary
            };

            try
            {
                _devContext.Add(tblTicketModifiedTable);
                _devContext.SaveChanges();
                return Ok(tblTicketModifiedTable);
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        // PUT api/<TicketModifiedTablesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TicketModifiedTablesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var table = _devContext.TblTicketModifiedTables.Where(x => x.Id == id).FirstOrDefault();

            var ticket = _devContext.TblTickets.Where(x => x.Id == table.TicketId).FirstOrDefault();
            
            if(ticket!=null)
            {
                if(ticket.Status=="Completed")
                {
                    return NotFound("Can't delete the object for a completed ticket");
                }
            }
            table.IsDeleted = true;
            _devContext.SaveChanges();

            return Ok();
        }
    }
}
