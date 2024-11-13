$(document).ready(function () {
    $('#registerForm').submit(function (e) {
        e.preventDefault();

        // Telefon numarasını doğru formatta almak için kontrol
        var phoneNumber = $('#PhoneNumber').val();
        if (!phoneNumber.startsWith("5")) {
            alert("Telefon numarası +90 formatında olmalıdır.");
            return;
        }

        // Form verilerini al
        var userData = {
            FirstName: $('#FirstName').val(),
            LastName: $('#LastName').val(),
            PhoneNumber: "+90" + phoneNumber,  // Telefon numarasına +90 ekleyin
            Password: $('#Password').val(),
        };

        // AJAX ile Register aksiyonuna veri gönder
        $.post('/User/Register', userData, function (response) {
            if (response.success) {
                alert("Doğrulama kodu gönderildi.");
                window.location.href = '/User/VerifyCode';  // Doğrulama sayfasına yönlendir
            } else {
                alert("Bir hata oluştu: " + response.message);
            }
        });
    });
});