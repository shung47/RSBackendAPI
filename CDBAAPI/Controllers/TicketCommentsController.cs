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
    public class TicketCommentsController : ControllerBase
    {
        private readonly DevContext _devContext;
        public TicketCommentsController(DevContext devContext)
        {
            _devContext = devContext;
        }
        // GET api/<TicketCommentsController>/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<TblTicketComment>> Get(int id)
        {
            try
            {
                var comments = _devContext.TblTicketComments.Where(x => x.TicketId == id);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        // POST api/<TicketCommentsController>
        [HttpPost]
        public IActionResult Post([FromBody] TblTicketComment value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var employeeId = claimsIdentity.FindFirst("EmployeeId").Value;

            var employee = claimsIdentity.FindFirst("Name").Value;

            TblTicketComment ticketComment = new TblTicketComment
            {
                TicketId = value.TicketId,
                CommentContent = value.CommentContent,
                Creator = employee,
                CreatorId = employeeId,
                LastModificationDateTime = DateTime.Now,
                CreateDateTime = DateTime.Now
            };

            try
            {
                _devContext.Add(ticketComment);
                _devContext.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(ex);
            }
        }

        // PUT api/<TicketCommentsController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TblTicketComment value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var comment = _devContext.TblTicketComments.Where(x => x.Id == id).FirstOrDefault();
            comment.CommentContent = value.CommentContent;
            comment.LastModificationDateTime = value.LastModificationDateTime;

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

        // DELETE api/<TicketCommentsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var comment = _devContext.TblTicketComments.Where(x => x.Id == id).FirstOrDefault();
            _devContext.TblTicketComments.Remove(comment);
            _devContext.SaveChanges();
        }
    }
}
