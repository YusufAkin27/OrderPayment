using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models.Request
{
    public class UpdateUserCardRequest
    {
        [Required(ErrorMessage = "Kart ID'si zorunludur.")]
        public int Id { get; set; } // Güncellenecek kartın benzersiz kimliği

        public string? CardNumber { get; set; } // Kart numarası (isteğe bağlı)
        public string? CardHolderName { get; set; } // Kart sahibinin adı (isteğe bağlı)
        public int? ExpiryMonth { get; set; } // Son kullanma ayı (isteğe bağlı)
        public int? ExpiryYear { get; set; } // Son kullanma yılı (isteğe bağlı)
        public string? CVV { get; set; } // CVC kodu (isteğe bağlı)
    }
}
