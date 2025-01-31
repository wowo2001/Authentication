using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Authentication.Models;
using Authentication.Services;

namespace Authentication.Controllers
{
    [Route("authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("createAccount")]
        public async Task<ActionResult<LoginResponse>> CreateAccount([FromBody] PasswordLoginRequest createAccountRequest)
        {
            var response = await _authenticationService.CreateAccount(createAccountRequest);
            return Ok(response);
        }
        [HttpPost("passwordLogin")]
        public async Task<ActionResult<LoginResponse>> PasswordLogin([FromBody] PasswordLoginRequest passwordLoginRequest)
        {
            var response = await _authenticationService.PasswordLogin(passwordLoginRequest);
            return Ok(response);
        }
        [HttpPost("tokenLogin")]
        public async Task<ActionResult<LoginResponse>> TokenLogin([FromBody] TokenLoginRequest tokenLoginRequest)
        {
            var response = await _authenticationService.TokenLogin(tokenLoginRequest);
            return Ok(response);
        }
        [HttpPost("getUsername")]
        public async Task<ActionResult<string>> getUsername([FromBody] TokenLoginRequest tokenLoginRequest)
        {
            var response = await _authenticationService.GetUserName(tokenLoginRequest);
            return Ok(response);
        }
    }
}
