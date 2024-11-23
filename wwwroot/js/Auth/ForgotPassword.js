document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("forgotPasswordForm");
    const errorMessage = document.getElementById("error-message");
    const phoneInput = document.getElementById("phone");

    // Telefon numarasını formatlama (xxx) xxx xx xx
    phoneInput.addEventListener("input", (event) => {
        let input = event.target.value.replace(/\D/g, ""); // Sadece rakamları al
        let formatted = "";

        if (input.length > 0) {
            formatted += "(" + input.substring(0, 3); // İlk 3 haneyi al
        }
        if (input.length >= 4) {
            formatted += ") " + input.substring(3, 6); // 4-6 arasını al
        }
        if (input.length >= 7) {
            formatted += " " + input.substring(6, 8); // 7-8 arasını al
        }
        if (input.length >= 9) {
            formatted += " " + input.substring(8, 10); // 9-10 arasını al
        }

        event.target.value = formatted; // Formatlı değeri inputa yaz
    });

    // Form gönderimi
    form.addEventListener("submit", async (event) => {
        event.preventDefault(); // Formun varsayılan gönderim işlemini durdur

        const phoneNumber = phoneInput.value.replace(/\D/g, ""); // Sadece rakamları al

        // Telefon numarasının geçerli formatta olup olmadığını kontrol et
        const phoneRegex = /^[5][0-9]{9}$/;
        if (!phoneRegex.test(phoneNumber)) {
            errorMessage.textContent = "Lütfen geçerli bir telefon numarası giriniz.";
            errorMessage.style.color = "red";
            return;
        }

        // API'ye istek gönder
        try {
            const response = await fetch("/Auth/ForgotPassword", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ phoneNumber: "+90" + phoneNumber }), // Telefon numarasına "+90" ekle
            });

            const result = await response.json();

            if (response.ok && result.success) {
                // Başarılı ise doğrulama sayfasına yönlendir
                window.location.href = "/Auth/VerifyForgotPasswordCode";
            } else {
                // Hata durumunda mesajı göster
                errorMessage.textContent = result.message || "Bir hata oluştu. Lütfen tekrar deneyin.";
                errorMessage.style.color = "red";
            }
        } catch (error) {
            // Ağ hatası veya beklenmedik bir hata
            console.error("Hata:", error); // Hata loglama
            errorMessage.textContent = "Bir ağ hatası oluştu. Lütfen tekrar deneyin.";
            errorMessage.style.color = "red";
        }
    });
});
