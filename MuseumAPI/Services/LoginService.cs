using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MuseumAPI.Data;
using MuseumAPI.Model;
using MuseumAPI.Services;
using System.Security.Cryptography;

public class LoginService : ILoginService
{
    private readonly AppDbContext _context;

    public LoginService(AppDbContext context)
    {
        _context = context;
    }

    //public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    //{
    //    using (var hmac = new HMACSHA512(storedSalt))
    //    {
    //        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    //        return computedHash.SequenceEqual(storedHash);
    //    }
    //}
    public static bool VerifyPasswordHash( string password, byte[] storedHash,byte[] storedSalt)
    {
        using var hmac = new HMACSHA256(storedSalt);
        var computedHash = hmac.ComputeHash(
            System.Text.Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(storedHash);
    }

    public LoginResponseDto Login(LoginRequestDto request)
    {
        try
        {
            var emailParam = new SqlParameter("@Email", request.email);

            // Get user info from DB
            var user = _context.LoginResponse.FromSqlRaw("EXEC LoginUser @Email", emailParam) .AsEnumerable().FirstOrDefault();

            if (user == null)
                return null; // User not found

            // Verify password
            bool isValidPassword =VerifyPasswordHash(request.password, user.PasswordHash, user.PasswordSalt);


            if (!isValidPassword)
                return null; // Invalid password

            // Remove sensitive info before returning
            user.PasswordHash = null;
            user.PasswordSalt = null;

            return user;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
