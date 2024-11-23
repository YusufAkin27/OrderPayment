$(document).ready(function () {
    $('#registerForm').submit(function (e) {
        e.preventDefault();

        // Telefon numarası kontrolü
        const phoneNumber = $('#phone').val()?.trim() || ''; // Güvenli şekilde al
        if (!phoneNumber) {
            alert("Telefon numarası boş bırakılamaz.");
            return;
        }
        const cleanedPhone = phoneNumber.replace(/\D/g, ''); // Telefon numarasını temizle (sadece rakamları al)
        if (!validatePhoneNumber(cleanedPhone)) {
            alert("Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.");
            return;
        }

        // Kullanıcı verilerini hazırlayın
        const userData = {
            FirstName: $('#FirstName').val()?.trim() || '',
            LastName: $('#LastName').val()?.trim() || '',
            PhoneNumber: `+90${cleanedPhone}`, // Telefon numarasına +90 eklenir
            Password: $('#password').val()?.trim() || '',
        };

        // Form doğrulama
        if (!validateFormData(userData)) {
            return;
        }

        // Sunucuya istek gönder
        sendRegisterRequest(userData);
    });

    // Telefon numarasını doğrulayan fonksiyon
    function validatePhoneNumber(phoneNumber) {
        return phoneNumber.startsWith("5") && phoneNumber.length === 10;
    }

    // Form verilerini doğrulayan fonksiyon
    function validateFormData(data) {
        if (!data.FirstName || !data.LastName || !data.PhoneNumber || !data.Password) {
            alert("Lütfen tüm alanları doldurun.");
            return false;
        }

        if (data.Password.length < 6) {
            alert("Şifre en az 6 karakter uzunluğunda olmalıdır.");
            return false;
        }

        return true;
    }

    // Sunucuya kayıt isteği gönderen fonksiyon
    function sendRegisterRequest(userData) {
        fetch('/Auth/Register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
        })
            .then(response => {
                console.log('Response:', response);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Data:', data);
                if (data.success) {
                    alert("Doğrulama kodu gönderildi. Doğrulama sayfasına yönlendiriliyorsunuz.");
                    window.location.href = '/Auth/VerifyCode';
                } else {
                    alert(`Bir hata oluştu: ${data.message}`);
                }
            })
            .catch(error => {
                console.error('Fetch Error:', error);
                alert("Sunucu ile iletişim kurulurken bir hata oluştu. Lütfen tekrar deneyin.");
            });
    }

    // Telefon numarasını formatlama (xxx) xxx xx xx
    $('#phone').on('input', function (event) {
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
    $('#phone').on('keydown', function (event) {
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
});
