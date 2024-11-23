using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OrderPayment.Models
{
    public class VerificationCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik artış ayarı

        public int VerificationCodeId { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "Verification code must be 6 characters.")]
        public string Code { get; set; } = string.Empty;  // Doğrulama kodu

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;  // Kod gönderildiği zaman

        // Geçerlilik süresi saniye cinsinden
        [Required]
        public int ExpiryInSeconds { get; set; }  // Kodun geçerlilik süresi saniye cinsinden

        // Foreign Key - Kullanıcıya ait doğrulama kodu
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }  // Doğrulama kodunun bağlı olduğu kullanıcı

        // Geçerlilik süresi kontrol fonksiyonu
        public bool IsExpired()
        {
            var currentTime = DateTime.UtcNow;
            var expiryTime = SentAt.AddSeconds(ExpiryInSeconds); // SentAt + ExpiryInSeconds
            return currentTime > expiryTime;
        }

        // Kalan süreyi saniye cinsinden döndüren fonksiyon
        public int GetRemainingTime()
        {
            var currentTime = DateTime.UtcNow;
            var expiryTime = SentAt.AddSeconds(ExpiryInSeconds); // SentAt + ExpiryInSeconds
            var remainingTime = (int)(expiryTime - currentTime).TotalSeconds;

            return remainingTime > 0 ? remainingTime : 0; // Süre 0'dan küçükse 0 döndür
        }
    }
}
