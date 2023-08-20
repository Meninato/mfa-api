using MfaApi.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MfaApi.Controllers;

[Controller]
public class BaseController : ControllerBase
{
    // returns the current authenticated account (null if not logged in)
    public Account Account => (Account)HttpContext.Items["Account"];
}
