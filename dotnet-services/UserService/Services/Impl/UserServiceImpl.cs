using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;
using UserService.Kafka;

namespace UserService.Services.Impl;

public class UserServiceImpl : IUserService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly UserEventProducer _producer;

    public UserServiceImpl(AppDbContext db, JwtService jwt, UserEventProducer producer)
    {
        _db = db;
        _jwt = jwt;
        _producer = producer;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already registered");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _producer.SendUserRegisteredEventAsync(user.Email);

        var token = _jwt.GenerateToken(user.Email, user.Role);
        return new AuthResponse { Token = token, Username = user.Username, Email = user.Email };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new Exception("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new Exception("Invalid credentials");

        var token = _jwt.GenerateToken(user.Email, user.Role);
        return new AuthResponse { Token = token, Username = user.Username, Email = user.Email };
    }
}
