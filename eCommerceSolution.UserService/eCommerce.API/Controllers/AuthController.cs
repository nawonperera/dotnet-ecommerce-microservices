using eCommerce.Core.DTO;
using eCommerce.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")] //api/auth
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUsersService _userService;

    public AuthController(IUsersService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")] //POST api/auth
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        //check for invalid registerRequest
        if(registerRequest == null)
        {
            return BadRequest("Invalid registration data");
        }

        //Call the userService to handle registration
        AuthenticationResponse? authenticationResponse = await _userService.Register(registerRequest);

        if(authenticationResponse is null || authenticationResponse.Success == false)
        {
            return BadRequest(authenticationResponse);
        }

        return Ok(authenticationResponse);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        //check for invalid loginResponse
        if(loginRequest == null)
        {
            return BadRequest("Invalid login data");
        }

        AuthenticationResponse? authenticationResponse = await _userService.Login(loginRequest);

        if(authenticationResponse is null || authenticationResponse.Success == false)
        {
            return Unauthorized(authenticationResponse);
        }

        return Ok(authenticationResponse);
    }




}
