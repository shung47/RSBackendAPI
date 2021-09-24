﻿using CDBAAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
        public IActionResult POST(User value)
        {
            var user = (from a in _devContext.Users
                        where a.Email == value.Email
                        && a.Password == value.Password
                        select a).SingleOrDefault();

            if(user ==null)
            {
                return NotFound();
            }

            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("Role", user.Role)
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

    }

    public class UserResponse
    {
        public string Token { get; set; }

    }
}
