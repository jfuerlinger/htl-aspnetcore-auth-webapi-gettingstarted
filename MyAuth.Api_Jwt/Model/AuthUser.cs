using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAuth.Api_Jwt.Model
{
  public class AuthUser
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string UserRole { get; set; }
  }
}
