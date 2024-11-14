document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("forgotPasswordForm");
    const errorMessage = document.getElementById("error-message");

    form.addEventListener("submit", async (event) => {
        event.preventDefault(); // Formun varsayılan gönderim işlemini durdur

        const phoneNumber = document.getElementById("phone").value;

        // Telefon numarasının geçerli formatta olup olmadığını kontrol et
        const phoneRegex = /^[5][0-9]{9}$/;
        if (!phoneRegex.test(phoneNumber)) {
            errorMessage.textContent = "Lütfen geçerli bir telefon numarası giriniz.";
            errorMessage.style.color = "red";
            return;
        }

        // API'ye istek gönder
        try {
            const response = await fetch("/User/ForgotPassword", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ phoneNumber: "+90" + phoneNumber }), // Telefon numarasına "+90" ekle
            });

            const result = await response.json();

            if (response.ok && result.success) {
                // Başarılı ise doğrulama sayfasına yönlendir
                window.location.href = "/User/VerifyForgotPasswordCode";
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
