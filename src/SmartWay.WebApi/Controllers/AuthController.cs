using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartWay.WebApi.DTO;
using SmartWay.WebApi.Entities;
using SmartWay.WebApi.Interfaces;

namespace SmartWay.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        ILogger<AuthController> logger, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(AuthDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            _logger.LogInformation("Email is already used");
            return BadRequest("Email is already used");
        }

        var user = new IdentityUser()
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
        };

        var res = await _userManager.CreateAsync(user, registerDto.Password);

        if (!res.Succeeded)
        {
            _logger.LogError("Error while creating user");
            return BadRequest("Error while creating user");
        }

        return _tokenService.CreateJwtToken(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(AuthDto authDto)
    {
        
        var user = await _userManager.FindByEmailAsync(authDto.Email);

        if (user == null)
            return Unauthorized();

        var res = await _signInManager.CheckPasswordSignInAsync(user, authDto.Password, false);

        if (!res.Succeeded)
            return Unauthorized();

        return _tokenService.CreateJwtToken(user);
    }
}