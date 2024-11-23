$(document).ready(function () {
    // Kategoriye göre ürünleri filtrele
    $(".dropdown-item").on("click", function (e) {
        e.preventDefault(); // Sayfa yönlendirmesini engelle
        const selectedCategory = $(this).text().trim(); // Seçilen kategoriyi al
        filterProductsByCategory(selectedCategory);
    });

    function filterProductsByCategory(category) {
        // "Tüm Ürünler" kategorisini seçtiyse tüm ürünleri göster
        if (category === "Tüm Ürünler") {
            $(".product-card-wrapper").show(); // Tüm ürünleri göster
        } else {
            $(".product-card-wrapper").each(function () {
                const productCategory = $(this).find(".card-category").text().trim();
                $(this).toggle(productCategory.includes(category)); // Belirtilen kategoriye göre ürünleri göster veya gizle
            });
        }
    }

    // Ürün arama
    $("#product-search").on("input", function () {
        const searchTerm = $(this).val().toLowerCase();
        $(".product-card-wrapper").each(function () {
            const productName = $(this).find(".card-title").text().toLowerCase();
            $(this).toggle(productName.includes(searchTerm)); // Arama terimiyle eşleşen ürünleri göster veya gizle
        });
    });

    // Sepete ürün ekleme
    $(".add-to-cart-btn").on("click", function () {
        const productId = $(this).data("product-id"); // Ürünün ID'sini al
        $.post("/Home/AddToCart", { productId: productId }, function (response) {
            if (response.success) {
                alert(response.message); // Başarılı mesajı göster
                updateCartUI(response.cartTotal); // Sepet arayüzünü güncelle
            } else {
                alert(response.message); // Başarısız mesajı göster
            }
        }).fail(function () {
            alert("Ürün sepete eklenirken bir hata oluştu."); // Hata mesajı
        });
    });

    // Sepet arayüzünü güncelleme fonksiyonu
    function updateCartUI(cartTotal) {
        $("#cart-total").text(cartTotal); // Sepet toplamını güncelle (örnek kullanım)
    }
});
