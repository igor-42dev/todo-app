using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Api.Models.Requests;
using TodoApp.Api.Models.Responses;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public LoginController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Senha, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Credenciais inválidas." });

        var token = GenerateJwtToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"]));

        return Ok(new LoginResponse
        {
            Token = token,
            Expiration = expiration
        });
    }

    [HttpGet("test")]
    [Authorize]
    public IActionResult Test()
    {
        return Ok("Autenticado com sucesso!");
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
