using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderPayment.Models
{
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Adres başlığı zorunludur.")]
        [StringLength(100, ErrorMessage = "Adres başlığı 100 karakterden uzun olamaz.")]
        public string AddressTitle { get; set; } = string.Empty; // Adres başlığı (örn. Ev, İş)

        [Required(ErrorMessage = "Sokak adresi zorunludur.")]
        [StringLength(200, ErrorMessage = "Sokak adresi 200 karakterden uzun olamaz.")]
        public string StreetAddress { get; set; } = string.Empty; // Sokak adresi

        [Required(ErrorMessage = "Mahalle adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Mahalle adı 100 karakterden uzun olamaz.")]
        public string Neighborhood { get; set; } = string.Empty; // Mahalle adı

        [Required(ErrorMessage = "İlçe adı zorunludur.")]
        [StringLength(100, ErrorMessage = "İlçe adı 100 karakterden uzun olamaz.")]
        public string District { get; set; } = string.Empty; // İlçe adı

        [Required(ErrorMessage = "Şehir adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Şehir adı 50 karakterden uzun olamaz.")]
        public string City { get; set; } = string.Empty; // Şehir adı

        [StringLength(500, ErrorMessage = "Adres tarifi 500 karakterden uzun olamaz.")]
        public string AddressDescription { get; set; } = string.Empty; // Adres tarifi (isteğe bağlı alan)

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; } = string.Empty; // Telefon numarası

        [Required(ErrorMessage = "Kullanıcı ID'si zorunludur.")]
        public int UserId { get; set; } // Kullanıcıya referans

        public User User { get; set; } // Kullanıcı ile ilişki
    }
}
