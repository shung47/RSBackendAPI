﻿using CDBAAPI.Models;
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
        public ActionResult<IEnumerable<TblTask>> Get()
        {
            var result = _devContext.TblTasks.Where(x=>x.IsDeleted==false);
            return Ok(result);
        }

        // GET api/<TasksController>/5
        [HttpGet("{id}")]
        public ActionResult<TblTask> Get(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            try
            {
                TblTask task = _devContext.TblTasks.Where(x => x.Id == id).First();
                return Ok(task);
            }
            catch(Exception ex)
            {
                return NotFound();
            }           
        }

        // POST api/<TasksController>
        [HttpPost]
        public IActionResult Post([FromBody] TblTask value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            TblTask task = new TblTask
            {
                TaskName = value.TaskName,
                Region = value.Region,
                Department = value.Department,
                Summary = value.Summary,
                ReferenceNumber = value.ReferenceNumber,
                IsDeleted = false
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
        public IActionResult Put(int id, [FromBody] TblTask value)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            var task = _devContext.TblTasks.Where(x => x.Id == id).First();

            task.TaskName = value.TaskName;
            task.Region = value.Region;
            task.Department = value.Department;
            task.Summary = value.Summary;
            task.ReferenceNumber = value.ReferenceNumber;
            
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
                var task = _devContext.TblTasks.Find(id);
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
