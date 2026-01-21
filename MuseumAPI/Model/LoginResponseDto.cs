namespace MuseumAPI.Model
{
    public class LoginResponseDto
    {
        public int? CustmId { get; set; }
        public string? Custm_name { get; set; }
        public string? Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
