let cart = {
    items: [
        { id: 'item1', price: 100, quantity: 1 }
    ]
};

function changeQuantity(itemId, change) {
    const item = cart.items.find(i => i.id === itemId);
    if (item) {
        item.quantity += change;
        if (item.quantity < 1) item.quantity = 1; // Miktar 1'den az olamaz
        updateCart();
    }
}

function removeItem(itemId) {
    cart.items = cart.items.filter(i => i.id !== itemId);
    updateCart();
}

function updateCart() {
    let totalAmount = 0;
    cart.items.forEach(item => {
        const totalPrice = item.price * item.quantity;
        totalAmount += totalPrice;

        // Ürün toplam fiyatını güncelle
        document.getElementById(`total-price-${item.id}`).innerText = `${totalPrice} TL`;

        // Ürün miktarını güncelle
        document.getElementById(`quantity-${item.id}`).innerText = item.quantity;
    });

    // Toplam tutarı güncelle
    document.getElementById('totalAmount').innerText = `${totalAmount} TL`;

    // Eğer sepet boşsa bir mesaj gösterebiliriz
    if (cart.items.length === 0) {
        document.getElementById('cart-summary').innerText = "Sepetiniz boş.";
    }
}

function completeOrder() {
    // Sepeti onayla butonuna tıklandığında yeni bir sayfaya yönlendirme
    window.location.href = '/Order/Confirmation'; // Bu URL'yi istediğiniz gibi güncelleyin
}
