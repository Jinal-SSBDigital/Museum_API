using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MuseumAPI.Data;

using MuseumAPI.Data;
using MuseumAPI.Model;
using System.Security.Cryptography;
using System.Text;

namespace MuseumAPI.Services
{
    public class CustomerService: ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA256();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }


        public async Task<bool> RegisterCustomerAsync(RegisterCustomerDto dto)
        {
            byte[] passwordHash;
            byte[] passwordSalt;

            CreatePasswordHash(dto.Password, out passwordHash, out passwordSalt);

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Custm_name", dto.Custm_name),
                new SqlParameter("@Email", dto.Email),
                new SqlParameter("@Mobile", dto.Mobile),
                new SqlParameter("@Address", dto.Address),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@PasswordSalt", passwordSalt),
                new SqlParameter("@Password", dto.Password), // ⚠️ Plain password
                new SqlParameter("@Latitude", dto.Latitude),
                new SqlParameter("@Longitude", dto.Longitude)
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.RegisterCuatomer " +
                    "@Custm_name, @Email, @Mobile, @Address, " +
                    "@PasswordHash, @PasswordSalt, @Password, " +
                    "@Latitude, @Longitude",
                    parameters
                );

                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
