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

// Telefon numarasını formatlama (xxx) xxx xx xx
document.getElementById('phone').addEventListener('input', function (event) {
    let input = event.target.value;
    let cleaned = input.replace(/\D/g, ''); // Sadece rakamları al
    let formatted = '';

    if (cleaned.length > 0) {
        formatted += '(' + cleaned.substring(0, 3); // İlk 3 haneyi al
    }
    if (cleaned.length >= 4) {
        formatted += ') ' + cleaned.substring(3, 6); // 4-6 arasını al
    }
    if (cleaned.length >= 7) {
        formatted += ' ' + cleaned.substring(6, 8); // 7-8 arasını al
    }
    if (cleaned.length >= 9) {
        formatted += ' ' + cleaned.substring(8, 10); // 9-10 arasını al
    }

    event.target.value = formatted; // Formatlı değeri inputa yaz
});

// Telefon silme işlemi sırasında doğru formatlamayı koruma
document.getElementById('phone').addEventListener('keydown', function (event) {
    const input = event.target.value;

    // Eğer Backspace tuşu ile siliniyorsa
    if (event.key === 'Backspace') {
        let cleaned = input.replace(/\D/g, ''); // Sadece rakamları al
        let formatted = '';

        // Son karakter silinirken formatı yeniden uygula
        if (cleaned.length > 1) {
            cleaned = cleaned.slice(0, -1); // Son karakteri sil
        } else {
            event.target.value = '';
            return;
        }

        if (cleaned.length > 0) {
            formatted += '(' + cleaned.substring(0, 3);
        }
        if (cleaned.length >= 4) {
            formatted += ') ' + cleaned.substring(3, 6);
        }
        if (cleaned.length >= 7) {
            formatted += ' ' + cleaned.substring(6, 8);
        }
        if (cleaned.length >= 9) {
            formatted += ' ' + cleaned.substring(8, 10);
        }

        event.target.value = formatted; // Formatlı değeri inputa yaz
        event.preventDefault(); // Varsayılan silme işlemini engelle
    }
});

// Form gönderimi
document.getElementById('loginForm').addEventListener('submit', function (event) {
    event.preventDefault();

    const phoneInput = document.getElementById('phone').value;
    const passwordInput = document.getElementById('password').value;
    const errorMessage = document.getElementById('error-message');

    // Telefon numarasını temizle: Sadece rakamları al
    const cleanedPhone = phoneInput.replace(/\D/g, ''); // 5551234567 gibi
    const phoneRegex = /^5\d{9}$/;

    if (!phoneRegex.test(cleanedPhone)) {
        errorMessage.textContent = 'Lütfen geçerli bir telefon numarası girin (xxx) xxx xx xx formatında).';
        return;
    }

    if (passwordInput.length < 6) {
        errorMessage.textContent = 'Şifre en az 6 karakter olmalıdır.';
        return;
    }

    errorMessage.textContent = '';

    // API'ye temizlenmiş numarayı ve şifreyi gönder
    fetch('/Auth/Login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ phoneNumber: "+90" + cleanedPhone, password: passwordInput })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.href = '/Auth/VerifyLoginCode';
            } else {
                errorMessage.textContent = data.message;
            }
        })
        .catch(error => {
            errorMessage.textContent = 'Bir hata oluştu. Lütfen tekrar deneyin.';
            console.error('Error:', error);
        });
});
