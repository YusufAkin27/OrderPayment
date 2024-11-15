document.addEventListener("DOMContentLoaded", () => {
    loadCartItems(); // Sayfa yüklendiğinde sepet bilgilerini getir
    setupPayNowButton(); // "Pay Now" butonu için event listener ekle
});
function formatCardNumber(input) {
    let value = input.value.replace(/\D/g, '');
    value = value.replace(/(.{4})/g, '$1-');
    input.value = value.substring(0, 19);
    toggleCardFlip();
    checkPaymentButton();
}

function formatExpiryDate(input) {
    let value = input.value.replace(/\D/g, '');
    value = value.replace(/(.{2})/g, '$1/');
    input.value = value.substring(0, 5);
    checkPaymentButton();
}

function toggleCardFlip() {
    const cardNumber = document.getElementById('card-number').value.replace(/-/g, '');
    if (cardNumber.length === 16) {
        document.getElementById('card').classList.add('expanded');
    } else {
        document.getElementById('card').classList.remove('expanded');
    }
}

function checkPaymentButton() {
    const cardNumber = document.getElementById('card-number').value.replace(/-/g, '');
    const expiryDate = document.getElementById('expiry-date').value;
    const cvv = document.getElementById('cvv').value;
    const cardOwner = document.getElementById('card-owner').value;

    if (cardNumber.length === 16 && expiryDate.length === 5 && cvv.length === 3 && cardOwner.length > 0) {
        document.getElementById('pay-button').disabled = false;
    } else {
        document.getElementById('pay-button').disabled = true;
    }
}
document.addEventListener("DOMContentLoaded", () => {
    loadCartItems(); // Sayfa yüklendiğinde sepet bilgilerini getir
    setupPayNowButton(); // "Pay Now" butonu için event listener ekle
});

// Sepet bilgilerini API'den getir ve sayfaya yerleştir
function loadCartItems() {
    fetch('/Cart/Cart')
        .then(response => {
            if (!response.ok) {
                throw new Error("Error fetching cart items");
            }
            return response.json();
        })
        .then(cart => {
            const orderItemsContainer = document.getElementById("order-items");
            const totalPriceContainer = document.getElementById("total-price");

            // Sepette ürün varsa listele
            if (cart && cart.cartItems.length > 0) {
                orderItemsContainer.innerHTML = ""; // Eski içeriği temizle
                let total = 0;

                cart.cartItems.forEach(item => {
                    total += item.quantity * item.price;

                    // Ürünleri listeye ekle
                    const orderItem = document.createElement("div");
                    orderItem.classList.add("order-item");
                    orderItem.innerHTML = `
                        <span>${item.productName} (x${item.quantity})</span>
                        <span>$${(item.quantity * item.price).toFixed(2)}</span>
                    `;
                    orderItemsContainer.appendChild(orderItem);
                });

                // Toplam fiyatı güncelle
                totalPriceContainer.textContent = `$${total.toFixed(2)}`;
            } else {
                // Sepet boşsa
                orderItemsContainer.innerHTML = "<p>Your cart is empty.</p>";
                totalPriceContainer.textContent = "$0.00";
            }
        })
        .catch(error => {
            console.error("Failed to load cart items:", error);
            const orderItemsContainer = document.getElementById("order-items");
            orderItemsContainer.innerHTML = "<p>Failed to load cart items. Please try again later.</p>";
        });
}

// "Pay Now" butonunun işlevselliği
function setupPayNowButton() {
    const payButton = document.getElementById("pay-button");

    payButton.addEventListener("click", () => {
        const cardNumber = document.getElementById("card-number").value.replace(/\s/g, "");
        const cardOwner = document.getElementById("card-owner").value.trim();
        const expiryDate = document.getElementById("expiry-date").value;
        const cvv = document.getElementById("cvv").value;

        if (!validateCardDetails(cardNumber, cardOwner, expiryDate, cvv)) {
            alert("Please provide valid card details.");
            return;
        }

        // POST isteğiyle siparişi onayla
        fetch('/ConfirmPayment', {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                cardNumber,
                cardOwner,
                expiryDate,
                cvv
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("Error confirming order");
                }
                return response.json();
            })
            .then(data => {
                alert("Order confirmed successfully!");
                // Başarılı işlemi göstermek için kullanıcıyı yönlendir
                window.location.href = "/Order/Details"; // Örnek: sipariş detaylarına yönlendirme
            })
            .catch(error => {
                console.error("Failed to confirm order:", error);
                alert("Failed to confirm order. Please try again later.");
            });
    });
}

// Kart detaylarını doğrulama
function validateCardDetails(cardNumber, cardOwner, expiryDate, cvv) {
    if (cardNumber.length !== 16 || isNaN(cardNumber)) {
        return false;
    }
    if (cardOwner.length === 0) {
        return false;
    }
    const expiryRegex = /^(0[1-9]|1[0-2])\/\d{2}$/;
    if (!expiryRegex.test(expiryDate)) {
        return false;
    }
    if (cvv.length !== 3 || isNaN(cvv)) {
        return false;
    }
    return true;
}


