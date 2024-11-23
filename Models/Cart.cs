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

            // To optimize performance, we will track the total amount as a property that is calculated when CartItems change
            public decimal TotalAmount { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date when the cart was created

            // Navigation property to the User who owns the cart
            public User User { get; set; }

        // Method to calculate the total amount from the cart items (make sure price and quantity are handled as decimals)
        public void UpdateTotalAmount()
        {
            TotalAmount = 0;

            foreach (var item in CartItems)
            {
                // Ensure both Quantity and Price are treated as decimals
                TotalAmount += item.Quantity * item.Price;
            }
        }
    }
    }
