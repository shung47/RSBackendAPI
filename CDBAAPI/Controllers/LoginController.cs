using CDBAAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

namespace CDBAAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DevContext _devContext;
        private readonly IConfiguration _configuration;

        public LoginController(DevContext devContext, IConfiguration configuration)
        {
            _devContext = devContext;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult POST(TblUser value)
        {

            var user = (from a in _devContext.TblUsers
                        where a.EmployeeId == value.EmployeeId
                        && a.Password == EncryptString(value.Password)
                        select a).SingleOrDefault();

            if (user == null)
            {
                return NotFound("Incorrect ID or password");
            }

            //if (DecryptString(user.Password)!=value.Password)
            //{
            //    return NotFound("Incorrect Email or Password");
            //}



            var claims = new List<Claim>
                {
                    new Claim("EmployeeId", user.EmployeeId),
                    new Claim("Name", user.Name)
                };

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
    }
    

    public class UserResponse
    {
        public string Token { get; set; }

    }
}
