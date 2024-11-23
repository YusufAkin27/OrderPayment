using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models.Request
{
    public class UpdateAddressRequest
    {
        [Required(ErrorMessage = "Adres ID'si zorunludur.")]
        public int Id { get; set; } // Adresin ID'si

        [Required(ErrorMessage = "Adres Başlığı zorunludur.")]
        [StringLength(200, ErrorMessage = "Adres başlığı 200 karakterden uzun olamaz.")]
        public string AddressTitle { get; set; } = string.Empty; // Sokak adresi

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
    }
}
