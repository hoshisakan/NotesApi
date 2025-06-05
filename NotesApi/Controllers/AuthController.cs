using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NotesApi.Data;
using NotesApi.Models;
using NotesApi.Services.IService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NoteAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _config;
    private readonly JwtService _jwtService;

    public AuthController(IUserService userService, IConfiguration config, JwtService jwtService)
    {
        _userService = userService;
        _config = config;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody]RegisterDto registerDto)
    {
        var success = await _userService.RegisterAsync(registerDto);
        if (!success)
            return BadRequest("User already exists.");

        return Ok("User registered.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        var token = await _userService.LoginAsync(dto);
        if (token == null)
            return Unauthorized("Invalid credentials");

        return Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] TokenResponseDto tokenDto)
    {
        var token = await _userService.RefreshAsync(tokenDto);
        if (token == null)
            return Unauthorized("Refresh token expired or invalid");

        return Ok(token);
    }
}