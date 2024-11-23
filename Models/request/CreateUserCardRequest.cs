using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models.Request
{
    public class CreateUserCardRequest
    {
        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Kart numarası 16 karakter uzunluğunda olmalıdır.")]
        public string CardNumber { get; set; } = string.Empty; // Kart numarası

        [Required(ErrorMessage = "Kart sahibi adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Kart sahibi adı 100 karakterden uzun olamaz.")]
        public string CardHolderName { get; set; } = string.Empty; // Kart sahibinin adı

        [Required(ErrorMessage = "Son kullanma ayı zorunludur.")]
        [Range(1, 12, ErrorMessage = "Son kullanma ayı 1 ile 12 arasında olmalıdır.")]
        public int ExpiryMonth { get; set; } // Son kullanma tarihi - Ay

        [Required(ErrorMessage = "Son kullanma yılı zorunludur.")]
        [Range(2024, 2100, ErrorMessage = "Son kullanma yılı geçerli bir değer olmalıdır.")]
        public int ExpiryYear { get; set; } // Son kullanma tarihi - Yıl

        [Required(ErrorMessage = "CVC kodu zorunludur.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVC kodu 3 karakter uzunluğunda olmalıdır.")]
        public string CVV { get; set; } = string.Empty; // Kart güvenlik kodu

    }
}
