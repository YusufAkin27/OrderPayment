document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("ResetPasswordForm");
    const errorMessage = document.getElementById("error-message");

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        // Fetch the new password values
        const newPassword = document.getElementById("newPassword").value;
        const confirmPassword = document.getElementById("confirmPassword").value;

        // Password matching validation
        if (newPassword !== confirmPassword) {
            errorMessage.textContent = "Şifreler eşleşmiyor!";
            errorMessage.style.color = "red";
            return;
        }

        // Send request to the API
        try {
            const response = await fetch(form.action, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ password: newPassword }),
            });

            const result = await response.json();

            if (response.ok && result.success) {
                // Redirect on success
                window.location.href = result.redirectUrl || "User/Login";
            } else {
                // Display error message
                errorMessage.textContent = result.message || "Şifre sıfırlama işlemi başarısız.";
                errorMessage.style.color = "red";
            }
        } catch (error) {
            // Network or unexpected error
            errorMessage.textContent = "Bir hata oluştu. Lütfen tekrar deneyin.";
            errorMessage.style.color = "red";
        }
    });
});

// Password toggle function
function togglePassword() {
    const passwordInputs = [document.getElementById("newPassword"), document.getElementById("confirmPassword")];
    passwordInputs.forEach((input) => {
        input.type = input.type === "password" ? "text" : "password";
    });

    const toggleIcons = document.querySelectorAll(".toggle-btn i");
    toggleIcons.forEach((icon) => {
        icon.classList.toggle("fa-eye");
        icon.classList.toggle("fa-eye-slash");
    });
}
