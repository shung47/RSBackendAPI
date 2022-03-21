using CDBAAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace CDBAAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DevContext _devContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _currentEnvironment;

        public object Configuration { get; private set; }

        public LoginController(DevContext devContext, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _devContext = devContext;
            _configuration = configuration;
            _currentEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public IActionResult POST(TblTicketUser value)
        {
            var loginInfo = (from a in _devContext.TblTicketLoginInfos
                            where a.LoginName == value.Name
                            && a.Inactive !="Y"
                            select a).SingleOrDefault();

            var user = (from a in _devContext.TblTicketUsers
                        where a.EmployeeId == loginInfo.Id
                        && a.Password == EncryptString(value.Password)
                        select a).SingleOrDefault();


            if (user == null)
            {
                return NotFound("Incorrect ID or password");
            }


            var claims = new List<Claim>
                {
                    new Claim("EmployeeId", user.EmployeeId),
                    new Claim("Name", user.Name),                                    
                };
            if (loginInfo.CanCreateTask != null)
            {
                claims.Add(new Claim("CanCreateTask", loginInfo.CanCreateTask));
            }
            else
            {
                claims.Add(new Claim("CanCreateTask", "N"));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]));

            var jwt = new JwtSecurityToken
        (
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        );
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            UserResponse userResponse = new UserResponse();
            userResponse.Token = token;
            return Ok(userResponse);
        }

        private string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_configuration["JWT:KEY"]);
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

        [HttpGet]
        public IActionResult GET()
        {
            return Ok("The connection works. The environment is " + _currentEnvironment.EnvironmentName);
        }

        [HttpGet("SendEmail/{employeeId}")]
        public IActionResult SendEmail(string employeeId)
        {

            var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == employeeId).FirstOrDefault();

            if(user==null)
            {
                return NotFound("Cannot find the user");
            }

            var mailMessage = new MimeMessage();

            var appUrl = "";
            if (_currentEnvironment.EnvironmentName == "UAT")
            {
                appUrl = _configuration["AppUrl:UAT"];
            }
            else if (_currentEnvironment.EnvironmentName == "Production")
            {
                appUrl = _configuration["AppUrl:Production"];
            }
            else
            {
                appUrl = _configuration["AppUrl:Development"];
            }

            Byte[] bytesEncode = System.Text.Encoding.UTF8.GetBytes(employeeId);
            var resetUrl = Convert.ToBase64String(bytesEncode);
            mailMessage.To.Add(new MailboxAddress(employeeId + "@avnet.com"));
            mailMessage.From.Add(new MailboxAddress("CDBA-AUTOMATION", "CDBA-AUTO@AVNET.COM"));
            mailMessage.Subject = "Reset Your Password";
            mailMessage.Body = new TextPart("plain")
            {
                Text = "Hi,\n\n" +
                "Please reset your password with the following link:\n" +
                 appUrl + "ResetPassword/" + resetUrl +
                "\n\nBest regards," +
                "\nCDBA Team"
            };

            if (mailMessage.To.Count() != 0)
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
        
        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword([FromBody] TblUser userInfo)
        {
            Byte[] bytesDecode = Convert.FromBase64String(userInfo.EmployeeId); 
            string id = System.Text.Encoding.UTF8.GetString(bytesDecode); 

            var user = _devContext.TblTicketUsers.Where(x => x.EmployeeId == id).FirstOrDefault();
            if(user==null)
            {
                return NotFound("Cannot get the user data");
            }
            user.Password = EncryptString(userInfo.Password);
            _devContext.SaveChanges();

            return Ok();
        }
    }


    public class UserResponse
    {
        public string Token { get; set; }

    }
}
