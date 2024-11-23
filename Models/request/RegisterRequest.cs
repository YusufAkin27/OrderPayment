using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models.request
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters.")]
        public string LastName { get; set; } = string.Empty;


        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }


        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; } = string.Empty;

    }
}
