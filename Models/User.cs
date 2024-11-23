using Iyzipay.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderPayment.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik artış ayarı

        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        public string Password { get; set; }


        [Required]
        [StringLength(256, ErrorMessage = "Password cannot be longer than 256 characters.")]
        public string PasswordHash { get; set; } = string.Empty;  // Şifreyi düz metin yerine hash'li olarak saklayacağız

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsActive { get; set; } = false;  // Kullanıcının aktif olup olmadığını belirten alan

        // Verification Code için ilişki
        [JsonIgnore]
        public List<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();

        // Foreign Key - One-to-Many relation with Orders
        public List<Order> Orders { get; set; } = new List<Order>();
        public Cart Cart { get; set; } // Kullanıcının sadece bir sepeti olacak

        [JsonIgnore]
        public ICollection<UserCard> UserCards { get; set; }

        [JsonIgnore]
        public List<Address> Addresses { get; set; } = new List<Address>();

    }
}
