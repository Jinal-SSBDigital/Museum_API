namespace MuseumAPI.Model
{
    public class RegisterCustomerDto
    {
        public string Custm_name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
