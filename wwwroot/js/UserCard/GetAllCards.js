// Kartları seçiyoruz
const cards = document.querySelectorAll('.card');

// Kartın tıklanarak dönmesini sağlayan fonksiyon
cards.forEach(card => {
    const cardInner = card.querySelector('.card-inner');

    // Kart tıklandığında dönüş animasyonunu başlat
    card.addEventListener('click', () => {
        if (cardInner.style.transform === 'rotateY(180deg)') {
            cardInner.style.transform = 'rotateY(0deg)';
        } else {
            cardInner.style.transform = 'rotateY(180deg)';
        }
        cardInner.style.transition = 'transform 0.8s ease-out';
    });
});

// Kartın arkasındaki CVV kısmının görünür olmasını sağlamak için
const cardBacks = document.querySelectorAll('.card-back');
cardBacks.forEach(cardBack => {
    cardBack.addEventListener('mouseenter', () => {
        const cvv = cardBack.querySelector('.cvv');
        if (cvv) {
            cvv.style.display = 'block';
            cvv.style.transition = 'opacity 0.3s ease';
            cvv.style.opacity = 1;
        }
    });

    cardBack.addEventListener('mouseleave', () => {
        const cvv = cardBack.querySelector('.cvv');
        if (cvv) {
            cvv.style.opacity = 0;
            cvv.style.transition = 'opacity 0.3s ease';
            setTimeout(() => {
                cvv.style.display = 'none';
            }, 300);
        }
    });
});

// Buton etkileşimleri için animasyon ekleme
const editButtons = document.querySelectorAll('.card .actions a.edit');
const deleteButtons = document.querySelectorAll('.card .actions a.delete');

// Butonlara hover animasyonu
editButtons.forEach(button => {
    button.addEventListener('mouseenter', () => {
        button.style.transform = 'scale(1.05)';
        button.style.transition = 'transform 0.3s ease';
    });

    button.addEventListener('mouseleave', () => {
        button.style.transform = 'scale(1)';
    });
});

deleteButtons.forEach(button => {
    button.addEventListener('mouseenter', () => {
        button.style.transform = 'scale(1.05)';
        button.style.transition = 'transform 0.3s ease';
    });

    button.addEventListener('mouseleave', () => {
        button.style.transform = 'scale(1)';
    });
});

// Butonlara tıklandığında animasyon ekleme
editButtons.forEach(button => {
    button.addEventListener('click', () => {
        button.style.transform = 'scale(0.95)';
        button.style.transition = 'transform 0.2s ease';
        setTimeout(() => {
            button.style.transform = 'scale(1)';
        }, 200);
    });
});

deleteButtons.forEach(button => {
    button.addEventListener('click', () => {
        button.style.transform = 'scale(0.95)';
        button.style.transition = 'transform 0.2s ease';
        setTimeout(() => {
            button.style.transform = 'scale(1)';
        }, 200);
    });
});
function editCard(cardId) {
    // Redirect to the UpdateCard page with the cardId
    window.location.href = '/UserCard/UpdateCard/' + cardId;
}

function deleteCard(cardId) {
    // Confirm deletion before proceeding
    if (!confirm("Bu kartı silmek istediğinizden emin misiniz?")) {
        return;  // If the user cancels, stop the deletion process
    }

    // Send AJAX request to delete the card
    $.ajax({
        url: "/UserCard/DeleteCard", // Your delete action's URL
        type: "DELETE",
        data: { cardId: cardId },  // Use DELETE method as per REST conventions
        success: function (response) {
            // On success, remove the card from the DOM and show confirmation
            $('#card-' + cardId).fadeOut(400, function () {
                $(this).remove();  // Remove the card element completely after fade out
            });
            alert(response);  // Optionally show success message
        },
        error: function (xhr, status, error) {
            // Show error message if something goes wrong
            if (xhr.responseJSON && xhr.responseJSON.message) {
                alert('Kart silinemedi: ' + xhr.responseJSON.message);  // Handle error message from server
            } else {
                alert('Kart silinemedi: Bir hata oluştu. Lütfen tekrar deneyin.');  // Default error message
            }
        }
    });
    function editCard(cardId) {
        console.log("Kart ID:", cardId);

        // Kart bilgilerini API'den al
        $.ajax({
            url: `/UserCard/GetCardDetails?id=${cardId}`, // ID'yi query string ile gönderiyoruz
            type: 'GET',
            dataType: 'json',
            success: function (cardDetails) {
                if (cardDetails) {
                    // Modal alanlarını doldur
                    document.getElementById("editCardId").value = cardDetails.id;
                    document.getElementById("editCardHolderName").value = cardDetails.cardHolderName;
                    document.getElementById("editCardNumber").value = cardDetails.cardNumber; // Dikkat: Tam numara döndürmek güvenlik riski olabilir
                    document.getElementById("editExpiryMonth").value = cardDetails.expiryMonth;
                    document.getElementById("editExpiryYear").value = cardDetails.expiryYear;
                    document.getElementById("editCvv").value = cardDetails.cvv; // CVV'yi asla tam göstermeyin

                    // Modal'ı aç
                    const modal = document.getElementById("editCardModal");
                    modal.style.display = "block";
                } else {
                    alert("Kart bilgileri bulunamadı.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Hata:", status, error);
                alert("Kart bilgileri alınamadı. Hata: " + xhr.responseJSON?.message || "Bilinmeyen bir hata oluştu.");
            }
        });
    }
    function submitCardUpdate() {
        // Modal içindeki alanlardan bilgileri al
        const cardId = document.getElementById("editCardId").value;
        const cardHolderName = document.getElementById("editCardHolderName").value;
        const cardNumber = document.getElementById("editCardNumber").value;
        const expiryMonth = document.getElementById("editExpiryMonth").value;
        const expiryYear = document.getElementById("editExpiryYear").value;
        const cvv = document.getElementById("editCvv").value;

        // API'ye gönderilecek veriler
        const updateRequest = {
            id: cardId,
            cardHolderName: cardHolderName,
            cardNumber: cardNumber,
            expiryMonth: parseInt(expiryMonth),
            expiryYear: parseInt(expiryYear),
            cvv: cvv
        };

        // API'ye PATCH isteği gönder
        $.ajax({
            url: '/UserCard/UpdateCard', // Backend endpoint
            type: 'PATCH',
            contentType: 'application/json',
            data: JSON.stringify(updateRequest), // JSON formatında veri gönderiyoruz
            success: function (response) {
                alert(response); // Başarı mesajını göster
                closeModal(); // Modal'ı kapat
                location.reload(); // Sayfayı yenileyerek yeni bilgileri göster
            },
            error: function (xhr, status, error) {
                console.error("Hata:", status, error);
                alert("Kart güncellenemedi. Hata: " + xhr.responseJSON?.message || "Bilinmeyen bir hata oluştu.");
            }
        });
    }

    // Modal'ı kapatma işlevi
    function closeModal() {
        const modal = document.getElementById("editCardModal");
        modal.style.display = "none";
    }

