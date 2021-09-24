using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
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
    public class UsersController : ControllerBase
    {
        private readonly DevContext _devContext;
        
        public UsersController(DevContext devContext)
        {
            _devContext = devContext;
        }
        // GET: api/<UsersController>
        //[HttpGet]
        //public ActionResult<IEnumerable<User>> Get()
        //{
        //    var result = _devContext.Users;
        //    return result;
        //}

        // GET api/<UsersController>/5
        //[HttpGet("{id}")]
        //public ActionResult<User> Get(int id)
        //{
        //    var result = _devContext.Users.Find(id);
        //    if(result==null)
        //    {
        //        return NotFound("Cannot find user");
        //    }
        //    return result;
        //}

        // POST api/<UsersController>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post([FromBody] User value)
        {
            var users = _devContext.Users.Where(a => a.Email==value.Email).Count();

            if(users==0)
            {
                _devContext.Users.Add(value);
                _devContext.SaveChanges();

                return Ok();
            }
            else
            {
                return NotFound("The account is already exist");
            }
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
