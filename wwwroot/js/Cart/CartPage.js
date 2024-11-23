// Ürün miktarını değiştirme fonksiyonu
function changeQuantity(productId, change) {
    const action = change > 0 ? "increment" : "decrement";

    const currentQuantity = parseInt($(`#quantity-${productId}`).text());
    const newQuantity = currentQuantity + change;

    if (newQuantity <= 0) {
        // Ürün miktarı sıfırlanıyor, ürünü hemen DOM'dan kaldır
        $(`#item-${productId}`).remove();
    }
    // Ajax isteği gönderme
    $.ajax({
        url: "/Cart/UpdateCart",   // İlgili controller metoduna isteği gönder
        type: "POST",              // POST isteği
        data: { productId: productId, action: action }, // Gönderilen veriler
        success: function (response) {
            if (response.success) {
                // Miktarı ve toplam fiyatı güncelle
                $(`#quantity-${productId}`).text(response.quantity);
                $(`#total-price-${productId}`).text(response.quantity * parseFloat($(`#price-${productId}`).data("price")).toFixed(2));
                updateTotalAmount(response.totalAmount);  // Toplam tutarı güncelle
            } else {
                alert(response.message);  // Hata mesajını göster
            }
        },
        error: function () {
            alert("Bir hata oluştu, lütfen tekrar deneyin.");
        }
    });
}

// Toplam tutarı güncelleme fonksiyonu
function updateTotalAmount(totalAmount) {
    // Toplam tutarı formatlayıp güncelle
    $("#totalAmount").text(totalAmount.toLocaleString("tr-TR", { style: "currency", currency: "TRY" }));
}

// Sepetten ürün kaldırma fonksiyonu
function removeItem(productId) {
    // Ürünü sepetten kaldırma işlemi için ajax isteği
    $.ajax({
        url: "/Cart/RemoveItem", // Sepetten ürün kaldırmak için yeni bir metod ekleyebilirsiniz
        type: "POST",
        data: { productId: productId },
        success: function (response) {
            if (response.success) {
                // Ürün başarıyla kaldırıldıysa, DOM'dan ürünü kaldır
                $(`#item-${productId}`).remove();
                updateTotalAmount(response.totalAmount);  // Toplam tutarı güncelle
            } else {
                alert(response.message);  // Hata mesajını göster
            }
        },
        error: function () {
            alert("Bir hata oluştu, lütfen tekrar deneyin.");
        }
    });
}
