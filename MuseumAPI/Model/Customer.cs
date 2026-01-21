using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuseumAPI.Model
{
    [Table("Customer")]
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustmId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Custm_name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Mobile { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime Createddate { get; internal set; }
        public DateTime? UpdateDate { get; set; }

        // Password hash and salt as byte arrays
        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        // Optional: only for testing, remove in production
        public string PlainTextPassword { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
