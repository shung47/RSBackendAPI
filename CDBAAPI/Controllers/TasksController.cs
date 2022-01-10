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
    public class TasksController : ControllerBase
    {
        private readonly DevContext _devContext;
        public TasksController(DevContext devContext)
        {
            _devContext = devContext;
        }
        // GET: api/<TasksController>
        [HttpGet]
        public ActionResult<IEnumerable<TblTicketTask>> Get()
        {
            var result = _devContext.TblTicketTasks.Where(x=>x.IsDeleted==false);
            return Ok(result.OrderByDescending(x=>x.Id));
        }

        // GET api/<TasksController>/5
        [HttpGet("{id}")]
        public ActionResult<TblTicketTask> Get(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            try
            {
                TblTicketTask task = _devContext.TblTicketTasks.Where(x => x.Id == id).First();
                return Ok(task);
            }
            catch(Exception ex)
            {
                return NotFound();
            }           
        }

        // POST api/<TasksController>
        [HttpPost]
        public IActionResult Post([FromBody] TblTicketTask value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var employee = claimsIdentity.FindFirst("Name").Value;

            TblTicketTask task = new TblTicketTask
            {
                TaskName = value.TaskName,
                Region = value.Region,
                Functions = value.Functions,
                Summary = value.Summary,
                ReferenceNumber = value.ReferenceNumber,
                CreatorId = employeeId,
                Creator = employee,
                CreatedDateTime = DateTime.Now,
                LastModificationDateTime = DateTime.Now,
                IsDeleted = false,
                Status = "ToDo",
                Priority = value.Priority
            };

            try
            {
                _devContext.Add(task);
                _devContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }

        }

        // PUT api/<TasksController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TblTicketTask value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var task = _devContext.TblTicketTasks.Where(x => x.Id == id).First();

            task.TaskName = value.TaskName;
            task.Region = value.Region;
            task.Functions = value.Functions;
            task.Summary = value.Summary;
            task.ReferenceNumber = value.ReferenceNumber;
            task.LastModificationDateTime = DateTime.Now;
            task.Priority = value.Priority;
            task.Status = value.Status;
            try
            {
                _devContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        // DELETE api/<TasksController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var task = _devContext.TblTicketTasks.Find(id);
                task.IsDeleted = true;
                _devContext.SaveChanges();
                return Ok();
            }
            catch(Exception ex)
            {
                return NotFound(ex);
            }

        }
    }
}
