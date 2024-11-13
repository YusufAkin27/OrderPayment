using System.ComponentModel.DataAnnotations;

namespace OrderPayment.Models
{
    public enum Category
    {
        SütÜrünleri,
        EtÜrünleri,
        Meyveler,
        Sebzeler
    }


    public enum Unit
    {
        Kilogram,
        Litre
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Fiyat pozitif bir değer olmalı")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Miktar sıfırdan büyük veya sıfır olmalı")]
        public int Quantity { get; set; }

        public Category Category { get; set; }
        
        public string? Image { get; set; } // Resim Base64 olarak kaydedilecek

        public Unit Unit { get; set; }



    }
}
