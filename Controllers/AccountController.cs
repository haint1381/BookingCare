using BookingCare.Common.Models;
using BookingCare.Common.Models.Request;
using BookingCare.Service.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookigCare.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SearchUser([FromBody] AccountGetsRequest request)
        {
            var response = await _accountService.Process(request);
            return (new JsonResult(response));
        }
    }
}
