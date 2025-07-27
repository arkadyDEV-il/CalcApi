using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CalcApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly JwtOptions _opts;
    private readonly IWebHostEnvironment _env;

    public AuthController(IOptions<JwtOptions> opts, IWebHostEnvironment env)
    {
        _opts = opts.Value;
        _env = env;
    }

    [HttpPost("token")]
    [AllowAnonymous] // important: no token required to mint a dev token
    public IActionResult Token()
    {
        if (!_env.IsDevelopment()) 
            return NotFound(); // dev-only

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: new[] { new Claim("role", "dev") },
            notBefore: now,
            expires: now.AddMinutes(_opts.ExpiryMinutes),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}