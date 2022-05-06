using CDBAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
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
        public IConfiguration Configuration { get; }

        public UsersController(DevContext devContext, IConfiguration configuration)
        {
            _devContext = devContext;
            Configuration = configuration;
        }


        //GET: api/<UsersController>
        [HttpGet]
        public ActionResult<IEnumerable<TblUser>> Get()
        {
            var users = _devContext.TblTicketUsers;

            var loginInfo = _devContext.TblTicketLoginInfos;

            var result = new List<TblUser>();
            foreach (var user in users)
            {
                var userInfo = loginInfo.Where(x => x.Id == user.EmployeeId).FirstOrDefault();
                bool isInActive = false;
                if (userInfo.Inactive == "Y")
                {
                    isInActive = true;
                }

                var tblUser = new TblUser
                {
                    Id = user.Id,
                    Password = user.Password,
                    EmployeeId = user.EmployeeId,
                    Name = user.Name,
                    Team = user.Team,
                    InActive = isInActive
                };
                result.Add(tblUser);             
            }

            return Ok(result.OrderBy(x=>x.Name));
        }

        [HttpGet("SA")]
        public ActionResult<IEnumerable<TblUser>>GetSA()
        {
            var loginInfo = _devContext.TblTicketLoginInfos.Where(x => x.Samaster == "Y"&&x.Inactive!="Y");
            List<TblUser> result = new List<TblUser>();
            foreach(var user in loginInfo)
            {
                
                TblUser u = new TblUser()
                {
                    Name = user.Name,
                    EmployeeId = user.Id
                };
                result.Add(u);
            }
            return result;
        }

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

        // POST api/<UsersController> SignUp
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Post([FromBody] TblUser value)
        {
            var users = _devContext.TblTicketUsers.Where(a => a.EmployeeId==value.EmployeeId);
            var validEmployees  = _devContext.TblTicketLoginInfos.Where(a=>a.Inactive == null && (a.Team == "HK"|| a.Team == "TW" || a.Team == "SG" || a.Team == "CN"));

            if(users.Count()>0)
            {
                return NotFound("Your employee ID is existing in the system");
            }

            if(users.Count() == 0)
            {
                if(validEmployees.Any(a => a.Id == value.EmployeeId))
                {
                    var validEmployee = validEmployees.Where(a => a.Id == value.EmployeeId).First();
                    TblTicketUser tblUser = new TblTicketUser()
                    {
                        EmployeeId = value.EmployeeId,
                        Password = EncryptString(value.Password),
                        Name = validEmployee.Name,
                        Team = validEmployee.Team
                    };
                    
                    _devContext.TblTicketUsers.Add(tblUser);
                    _devContext.SaveChanges();

                    return Ok();
                }
                else
                {
                    return NotFound("Sorry, you are not allowed to create an account in the system");
                }
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

        private string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Configuration["JWT:KEY"]);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
    }
}
