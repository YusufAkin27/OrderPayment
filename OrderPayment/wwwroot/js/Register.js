$(document).ready(function () {
    $('#registerForm').submit(function (e) {
        e.preventDefault();

        // Form verilerini al ve telefon numarasını doğru formatta kontrol et
        var phoneNumber = $('#PhoneNumber').val().trim();
        if (!phoneNumber.startsWith("5") || phoneNumber.length !== 10) {
            alert("Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.");
            return;
        }

        // Kullanıcı verilerini hazırlayın
        var userData = {
            FirstName: $('#FirstName').val().trim(),
            LastName: $('#LastName').val().trim(),
            PhoneNumber: "+90" + phoneNumber,  // Telefon numarasına +90 eklenir
            Password: $('#Password').val().trim(),
        };

        // Alanların boş olup olmadığını kontrol edin
        if (!userData.FirstName || !userData.LastName || !userData.PhoneNumber || !userData.Password) {
            alert("Lütfen tüm alanları doldurun.");
            return;
        }

        // Şifre uzunluğu kontrolü
        if (userData.Password.length < 6) {
            alert("Şifre en az 6 karakter uzunluğunda olmalıdır.");
            return;
        }

    
        $.post('/User/Register', userData, function (response) {
            if (response.success) {
                alert("Doğrulama kodu gönderildi. Doğrulama sayfasına yönlendiriliyorsunuz.");
                window.location.href = '/User/VerifyCode';  // Doğrulama sayfasına yönlendir
            } else {
                alert("Bir hata oluştu: " + response.message);
            }
        }).fail(function () {
            alert("Sunucu ile iletişim kurulurken bir hata oluştu. Lütfen tekrar deneyin.");
        });
    });
});
