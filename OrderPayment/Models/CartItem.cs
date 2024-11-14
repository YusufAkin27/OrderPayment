using System;

namespace OrderPayment.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; } // Foreign key to identify the cart
        public Cart Cart { get; set; }


        public int ProductId { get; set; } // Foreign key to identify the product
        public Product Product { get; set; }

        public decimal Quantity { get; set; } // Quantity of the product in the cart
        public decimal Price { get; set; } // Price per unit of the product
        public DateTime AddedAt { get; set; } // Date when the product was added to the cart
    }
}
