using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models
{
    public class LoginRequest
    {
        [Required]
        [Phone(ErrorMessage = "Geçersiz telefon numarası.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }
    }
}