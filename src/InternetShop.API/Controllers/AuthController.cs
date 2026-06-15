using InternetShop.API.Data;
using InternetShop.API.DTOs;
using InternetShop.API.Models;
using InternetShop.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternetShop.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt, LogService log) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Пользователь с таким email уже существует." });

        var user = new User
        {
            Name         = req.Name,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await log.InfoAsync("Auth", $"Регистрация: {user.Email}");

        return Ok(new AuthResponse(jwt.GenerateToken(user), user.Name, user.Email, user.Role.ToString()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный email или пароль." });

        await log.InfoAsync("Auth", $"Вход: {user.Email}");
        return Ok(new AuthResponse(jwt.GenerateToken(user), user.Name, user.Email, user.Role.ToString()));
    }
}
