document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("ResetPasswordForm");
    const errorMessage = document.getElementById("error-message");

    form.addEventListener("submit", (event) => {
        event.preventDefault();

        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        // Şifre doğrulama
        if (newPassword !== confirmPassword) {
            errorMessage.textContent = "Şifreler eşleşmiyor!";
            errorMessage.style.color = "red";
            return;
        }

        if (newPassword.length < 6) {
            errorMessage.textContent = "Şifre en az 6 karakter olmalıdır.";
            errorMessage.style.color = "red";
            return;
        }

        // API'ye POST isteği
        fetch('/Auth/ResetPassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password: newPassword })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.location.href = data.redirectUrl || '/Auth/Login';
                } else {
                    errorMessage.textContent = data.message || "Şifre sıfırlama işlemi başarısız.";
                    errorMessage.style.color = "red";
                }
            })
            .catch(error => {
                errorMessage.textContent = "Bir hata oluştu. Lütfen tekrar deneyin.";
                errorMessage.style.color = "red";
                console.error("Error:", error);
            });
    });
});

// Şifre göster/gizle
function togglePassword() {
    const inputs = document.querySelectorAll("input[type='password']");
    inputs.forEach(input => {
        input.type = input.type === "password" ? "text" : "password";
    });

    const icons = document.querySelectorAll(".toggle-btn i");
    icons.forEach(icon => {
        icon.classList.toggle("fa-eye");
        icon.classList.toggle("fa-eye-slash");
    });
}
