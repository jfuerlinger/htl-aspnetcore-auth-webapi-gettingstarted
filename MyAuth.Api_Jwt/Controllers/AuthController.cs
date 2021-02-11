using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyAuth.Api_Jwt.DataTransferObjects;
using MyAuth.Api_Jwt.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace MyAuth.Api_Jwt.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IConfiguration _config;
    private readonly IList<AuthUser> _authUsers;

    public AuthController(IConfiguration configuration)
    {
      _config = configuration;

      _authUsers = new List<AuthUser>()
      {
        new AuthUser {Email="admin@htl.at",
        Password = AuthUtils.GenerateHashedPassword("12345"),
        UserRole="Admin"},
        new AuthUser {Email="user@htl.at",
        Password = AuthUtils.GenerateHashedPassword("5678"),
        UserRole="User"},
        new AuthUser {Email="norole@htl.at",
        Password = AuthUtils.GenerateHashedPassword("7890")
        },
      };
    }
    
    [Route("login")]
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Login([FromBody] LoginRequestDto model)
    {
      var authUser = _authUsers.SingleOrDefault(u => u.Email == model.Email);
      if (authUser == null)
      {
        return Unauthorized();
      }

      if (!AuthUtils.VerifyPassword(model.Password, authUser.Password))
      {
        return Unauthorized();
      }

      var tokenString = GenerateJwtToken(authUser);
      return Ok(new
      {
        auth_token = tokenString,
        userMail = authUser.Email
      });
    }


    private string GenerateJwtToken(AuthUser userInfo)
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
      
      var authClaims = new List<Claim>();
      authClaims.Add(new Claim(ClaimTypes.Email, userInfo.Email));
      if (!string.IsNullOrEmpty(userInfo.UserRole))
      {
        authClaims.Add(new Claim(ClaimTypes.Role, userInfo.UserRole));
      }

      var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: authClaims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: credentials);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}
