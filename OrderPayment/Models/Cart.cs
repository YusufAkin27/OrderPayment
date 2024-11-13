using System;
using System.Collections.Generic;

namespace OrderPayment.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to identify the user

        // Navigation property to link to the CartItems (products in the cart)
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        // To optimize performance, we will track total amount as a property that is calculated when CartItems change
        public double TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date when the cart was created

        // Navigation property to the User who owns the cart
        public User User { get; set; }

    }
   
}
