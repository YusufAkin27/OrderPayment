function togglePassword() {
    const passwordInput = document.getElementById('password');
    const toggleBtn = document.querySelector('.toggle-btn i');
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleBtn.classList.remove('fa-eye');
        toggleBtn.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        toggleBtn.classList.remove('fa-eye-slash');
        toggleBtn.classList.add('fa-eye');
    }
}
// Şifre Görünürlüğünü Değiştirme Fonksiyonu
function togglePassword() {
    const passwordInput = document.getElementById('password');
    const toggleIcon = document.querySelector('.toggle-btn i');
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    }
}
document.getElementById('loginForm').addEventListener('submit', function (event) {
    event.preventDefault();

    const phoneInput = document.getElementById('phone').value;
    const passwordInput = document.getElementById('password').value;
    const errorMessage = document.getElementById('error-message');
    const phoneRegex = /^5\d{9}$/;

    if (!phoneRegex.test(phoneInput)) {
        errorMessage.textContent = 'Lütfen geçerli bir telefon numarası girin (5xxxxxxxxx formatında).';
        return;
    }
    if (passwordInput.length < 6) {
        errorMessage.textContent = 'Şifre en az 6 karakter olmalıdır.';
        return;
    }

    errorMessage.textContent = '';

    fetch('/User/Login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ phoneNumber: "+90" + phoneInput, password: passwordInput })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.href = '/User/VerifyLoginCode';
            } else {
                errorMessage.textContent = data.message;
            }
        })
        .catch(error => {
            errorMessage.textContent = 'Bir hata oluştu. Lütfen tekrar deneyin.';
            console.error('Error:', error);
        });
});