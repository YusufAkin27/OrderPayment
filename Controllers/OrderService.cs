using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using OrderPayment.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Address = Iyzipay.Model.Address;

public class OrderService
{
    private readonly Iyzipay.Options _options;

    public OrderService()
    {
        // Iyzipay test ortamı ayarları
        _options = new Iyzipay.Options
        {
            ApiKey = "sandbox-kAPSicMmtrJtsgiCE7ND9GgS11UyiNAc", // Test API Key
            SecretKey = "sandbox-wpktMn3R7h9Wy50YjbrRSE04FLhJm2j4", // Test Secret Key
            BaseUrl = "https://sandbox-api.iyzipay.com" // Test ortamı için URL
        };
    }

    public string PlaceOrder(User user, OrderPayment.Models.Address address, List<CartItem> cartItems,UserCard card)
    {
        if (cartItems == null || !cartItems.Any())
        {
            throw new Exception("Sepet boş. Sipariş verilemez.");
        }

        // 1. Ödeme işlemi için gerekli nesneleri oluştur
        var paymentRequest = new CreatePaymentRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = Guid.NewGuid().ToString(),
            Price = cartItems.Sum(x => x.Quantity * x.Price).ToString("F2"), // Toplam tutar
            PaidPrice = cartItems.Sum(x => x.Quantity * x.Price).ToString("F2"), // Ödenen tutar
            Currency = Currency.TRY.ToString(),
            Installment = 1, // Tek çekim
            BasketId = Guid.NewGuid().ToString(),
            PaymentChannel = PaymentChannel.WEB.ToString(),
            PaymentGroup = PaymentGroup.PRODUCT.ToString()
        };

        // 2. Kullanıcı kart bilgisi (test için dummy bilgiler)
        paymentRequest.PaymentCard = new PaymentCard
        {
            CardHolderName = card.CardHolderName,
            CardNumber = card.CardNumber,
            ExpireMonth = card.ExpiryMonth.ToString("D2"), // İki haneli string format (ör. 02, 11)
            ExpireYear = card.ExpiryYear.ToString(),      // Yıl string olarak
            Cvc = card.CVV,
            RegisterCard = 0 // Kartı kaydetme (0: Kaydetme, 1: Kaydet)
        };


        // 3. Fatura bilgileri
        paymentRequest.Buyer = new Buyer
        {
            Id = user.Id.ToString(),
            Name = user.FirstName,
            Surname = user.LastName,
            GsmNumber = user.PhoneNumber,
            Email = "test@example.com", // Email
            IdentityNumber = "12345678901", // TC Kimlik
            LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationDate = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationAddress = $"{address.StreetAddress}, {address.Neighborhood}, {address.District}",
            Ip = "85.34.78.112", // Test IP
            City = address.City,
            Country = "Turkey", // Ülke
            ZipCode = "34000" // Posta kodu
        };

        // 4. Teslimat adresi
        paymentRequest.ShippingAddress = new Address
        {
            ContactName = $"{user.FirstName} {user.LastName}",
            City = address.City,
            Country = "Turkey",
            Description = $"{address.StreetAddress}, {address.Neighborhood}, {address.District}"
        };

        // 5. Fatura adresi
        paymentRequest.BillingAddress = new Address
        {
            ContactName = $"{user.FirstName} {user.LastName}",
            City = address.City,
            Country = "Turkey",
            Description = $"{address.StreetAddress}, {address.Neighborhood}, {address.District}"
        };

        // 6. Sepet öğeleri
        paymentRequest.BasketItems = cartItems.Select(cartItem => new BasketItem
        {
            Id = cartItem.ProductId.ToString(),
            Name = cartItem.Product.Name,
            Category1 = "Genel", // Kategori (opsiyonel)
            Price = (cartItem.Quantity * cartItem.Price).ToString("F2"), // Ürün tutarı
            ItemType = BasketItemType.PHYSICAL.ToString() // Fiziksel ürün
        }).ToList();

        // 7. Ödeme işlemini başlat
        var payment = Payment.Create(paymentRequest, _options);

        // 8. Ödeme sonucu kontrolü
        if ("success".Equals(payment.Status))
        {
            return $"Ödeme başarıyla tamamlandı. Sipariş Numarası:";
        }
        else
        {
            throw new Exception($"Ödeme başarısız:");
        }

    }
}
