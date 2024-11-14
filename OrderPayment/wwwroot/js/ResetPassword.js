document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("ResetPasswordForm");
    const errorMessage = document.getElementById("error-message");

    form.addEventListener("submit", async (event) => {
        event.preventDefault(); // Formun varsayılan gönderim işlemini durdur

        // Şifreleri al
        const passwordInputs = document.querySelectorAll("input[type='password']");
        const password = passwordInputs[0].value;
        const confirmPassword = passwordInputs[1].value;

        // Şifre doğrulama
        if (password !== confirmPassword) {
            errorMessage.textContent = "Şifreler eşleşmiyor!";
            errorMessage.style.color = "red";
            return;
        }

        // API'ye istek gönder
        try {
            const response = await fetch(form.action, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ password: password }),
            });

            const result = await response.json();

            if (response.ok && result.success) {
                // Başarılı ise yönlendirme yap
                window.location.href = result.redirectUrl || "User/Login";
            } else {
                // Hata durumunda mesajı göster
                errorMessage.textContent = result.message || "Şifre sıfırlama işlemi başarısız.";
                errorMessage.style.color = "red";
            }
        } catch (error) {
            // Ağ hatası veya beklenmedik bir hata
            errorMessage.textContent = "Bir hata oluştu. Lütfen tekrar deneyin.";
            errorMessage.style.color = "red";
        }
    });
});

// Şifre göster/gizle fonksiyonu
function togglePassword() {
    const passwordInputs = document.querySelectorAll("input[type='password']");
    passwordInputs.forEach((input) => {
        if (input.type === "password") {
            input.type = "text";
        } else {
            input.type = "password";
        }
    });

    const toggleIcons = document.querySelectorAll(".toggle-btn i");
    toggleIcons.forEach((icon) => {
        icon.classList.toggle("fa-eye");
        icon.classList.toggle("fa-eye-slash");
    });
}
