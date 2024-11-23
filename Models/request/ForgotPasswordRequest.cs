using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models.request
{
    public class ForgotPasswordRequest
    {
        [Required]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string PhoneNumber { get; set; }
    }
}
