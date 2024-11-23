using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderPayment.Models
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik artış ayarı

        public int AdminId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; }

        public string Role { get; set; }
        

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // List of Users managed by Admin
        public List<User> ManagedUsers { get; set; } = new List<User>();

        // List of Orders managed by Admin
        public List<Order> ManagedOrders { get; set; } = new List<Order>();
    }
}
