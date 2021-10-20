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
            var result = _devContext.TblUsers;
            return Ok(result);
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
            var users = _devContext.TblUsers.Where(a => a.Email==value.Email).Count();
            var checkEmployeeId = _devContext.TblUsers.Where(a => a.EmployeeId == value.EmployeeId).Count();

            var identity = WindowsIdentity.GetCurrent().Name;
            if (!identity.Contains(value.EmployeeId))
            {
                return NotFound("Your employee ID doesn't match with your windows identity");
            }

            if (!value.Email.Contains("avnet.com"))
            {
                return NotFound("You have to register with an Avnet email");
            }

            if(checkEmployeeId>0)
            {
                return NotFound("Your employee ID is exist");
            }

            if(users==0)
            {
                value.Password = EncryptString(value.Password);
                _devContext.TblUsers.Add(value);
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
