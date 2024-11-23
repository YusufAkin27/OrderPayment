     document.getElementById("submitCard").addEventListener("click", async () => {
            // Form alanlarını al ve form verilerini hazırla
            const formData = {
                CardNumber: document.getElementById("CardNumber").value.trim(),
                CardHolderName: document.getElementById("CardHolderName").value.trim(),
                ExpiryMonth: document.getElementById("ExpiryMonth").value.trim(),
                ExpiryYear: document.getElementById("ExpiryYear").value.trim(),
                CVV: document.getElementById("CVV").value.trim(),
            };

            // Önce boş alanları kontrol et
            for (const key in formData) {
                if (!formData[key]) {
                    alert(`${key} alanı boş bırakılamaz!`);
                    return;
                }
            }

            try {
                // Fetch API ile POST isteği gönder
                const response = await fetch('@Url.Action("AddCard", "UserCard")', {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(formData),
                });

                // Yanıtı işle
                if (response.ok) {
                    alert("Kart başarıyla eklendi!");
                    // Kart ekleme başarılıysa yönlendir
                    window.location.href = "/UserCard/GetAllCards";
                } else {
                    // Sunucudan dönen hatayı işle
                    const errorData = await response.json();
                    alert(errorData.message || "Bir hata oluştu!");
                }
            } catch (error) {
                // Ağ hatalarını ele al
                console.error("Hata:", error);
                alert("Bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
            }
        });