// CSRF Token'ı al (ASP.NET Core için gerekli)
const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');

// Ürünü sepete ekle
function addToCart(productId, productName, productPrice) {
    $.ajax({
        url: '/Home/AddToCart', // Backend'deki CartController AddToCart aksiyonu
        type: 'POST',
        headers: {
            'RequestVerificationToken': csrfToken // CSRF Token gönder
        },
        data: {
            productId: productId,
            quantity: 1 // Varsayılan olarak 1 adet ekle
        },
        success: function (response) {
            if (response.success) {
                alert(`${productName} sepete eklendi.`);
                updateCartUI(response.cart); // Güncellenen sepeti UI'a yansıt
            } else {
                alert("Ürün sepete eklenirken hata oluştu: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Hatası:", error);
            alert("Sepete ürün eklenemedi. Lütfen tekrar deneyin.");
        }
    });
}

// Sepet açma/kapama işlevi
function toggleCart() {
    const cartPanel = document.getElementById("cart-panel");
    cartPanel.classList.toggle("open");
}

// Sepeti güncelle
function updateCartUI(cart) {
    const cartItemsContainer = document.getElementById('cart-items');
    const totalPriceElement = document.getElementById('total-price');

    if (!cart || cart.items.length === 0) {
        cartItemsContainer.innerHTML = '<tr><td colspan="5">Sepet boş.</td></tr>';
        totalPriceElement.innerText = 'Genel Toplam: 0.00 ₺';
        return;
    }

    let totalPrice = 0;
    cartItemsContainer.innerHTML = ''; // Mevcut listeyi temizle

    cart.items.forEach(item => {
        const itemTotal = item.price * item.quantity;
        totalPrice += itemTotal;

        const row = `
            <tr>
                <td>${item.name}</td>
                <td>${item.quantity}</td>
                <td>${item.price.toFixed(2)} ₺</td>
                <td>${itemTotal.toFixed(2)} ₺</td>
                <td>
                    <button onclick="removeFromCart(${item.id})">Sil</button>
                </td>
            </tr>
        `;
        cartItemsContainer.innerHTML += row;
    });

    totalPriceElement.innerText = `Genel Toplam: ${totalPrice.toFixed(2)} ₺`;
}

// Sepetten ürün silme
function removeFromCart(productId) {
    $.ajax({
        url: '/Cart/RemoveFromCart', // Backend'deki RemoveFromCart aksiyonu
        type: 'POST',
        headers: {
            'RequestVerificationToken': csrfToken
        },
        data: { productId: productId },
        success: function (response) {
            if (response.success) {
                alert("Ürün sepetten kaldırıldı.");
                updateCartUI(response.cart); // Güncellenen sepeti UI'a yansıt
            } else {
                alert("Ürün sepetten silinirken hata oluştu: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error("AJAX Hatası:", error);
            alert("Sepetten ürün silinemedi. Lütfen tekrar deneyin.");
        }
    });
}
