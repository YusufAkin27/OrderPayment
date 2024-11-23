document.addEventListener("DOMContentLoaded", function () {
    // Sipariş detay sayfasına gitme
    const detailButtons = document.querySelectorAll(".btn-info");
    detailButtons.forEach(button => {
        button.addEventListener("click", function () {
            const orderId = this.getAttribute("data-order-id");
            window.location.href = `/Order/OrderDetails/${orderId}`;
        });
    });

    // Siparişi iptal etme
    const cancelButtons = document.querySelectorAll(".btn-danger");
    cancelButtons.forEach(button => {
        button.addEventListener("click", function () {
            const orderId = this.getAttribute("data-order-id");
            if (confirm("Bu siparişi iptal etmek istediğinizden emin misiniz?")) {
                fetch(`/Order/CancelOrder/${orderId}`, {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value,
                        "Content-Type": "application/json"
                    }
                })
                    .then(response => {
                        if (response.ok) {
                            alert("Sipariş başarıyla iptal edildi.");
                            location.reload(); // Sayfayı yenile
                        } else {
                            return response.text().then(text => { throw new Error(text); });
                        }
                    })
                    .catch(error => {
                        alert("Siparişi iptal ederken bir hata oluştu: " + error.message);
                    });
            }
        });
    });
});
