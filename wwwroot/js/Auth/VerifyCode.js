$(document).ready(function () {
    // Kod giriş kutuları arasında otomatik geçiş ve silme işlemi
    $(".code-input").on("input", function (e) {
        const currentInput = $(this);
        const nextInput = currentInput.next(".code-input");
        const prevInput = currentInput.prev(".code-input");

        // Yalnızca tek bir karakter (rakam) kabul et
        if (/^[0-9]$/.test(currentInput.val())) {
            // Geçerli bir rakam girildiğinde otomatik olarak bir sonraki kutuya geç
            if (nextInput.length) {
                nextInput.focus();
            }
        } else {
            // Rakam değilse alanı temizle
            currentInput.val('');
        }
    });

    $(".code-input").on("keydown", function (e) {
        const currentInput = $(this);
        const prevInput = currentInput.prev(".code-input");

        // Backspace ile geri giderken alanı sil ve önceki inputa geç
        if (e.key === "Backspace") {
            if (!currentInput.val() && prevInput.length) {
                prevInput.focus().val(''); // Geri giderken önceki kutuyu da temizle
            }
        }
    });

    // İlk kutuya otomatik odaklan
    $(".code-input").first().focus();
});


// Kod doğrulama işlemi
$("#verifyButton").click(function () {
    let verificationCode = "";
    $(".code-input").each(function () {
        verificationCode += $(this).val();
    });

    $.post('/Auth/VerifyCode', { verificationCode: verificationCode }, function (response) {
        const alertClass = response.success ? 'alert-success' : 'alert-danger';
        $('#resultMessage').html('<div class="alert ' + alertClass + '">' + response.message + '</div>');

        if (response.success) {
            setTimeout(function () {
                window.location.href = '/Auth/Login';
            }, 1000);
        }
    });
});

// Kalan süreyi güncelleme ve zamanlayıcı başlatma
let intervalId;

function updateRemainingTime() {
    $.post('/Auth/GetRemainingTime', function (response) {
        if (response.success) {
            $('#remainingTime').text(response.remainingTime);

            if (response.remainingTime === "00:00") {
                $('#remainingTimeMessage').html('<div class="alert alert-danger">Doğrulama kodunun süresi doldu. Yeni bir kod alın.</div>');
                $('#resendButton').prop('disabled', false);
                clearInterval(intervalId);
            }
        } else {
            $('#remainingTimeMessage').html('<div class="alert alert-danger">' + response.message + '</div>');
        }
    });
}

function startTimer() {
    clearInterval(intervalId);
    intervalId = setInterval(updateRemainingTime, 1000);
}

// Başlangıçta kalan süreyi güncelle ve zamanlayıcıyı başlat
updateRemainingTime();
startTimer();

// Yeniden kod gönderme
$('#resendButton').click(function () {
    $.post('/Auth/ResendVerificationCode', function (response) {
        if (response.success) {
            $('#resultMessage').html('<div class="alert alert-success">Yeni doğrulama kodu gönderildi.</div>');
            $('#resendButton').prop('disabled', true); // Yeniden gönder butonunu devre dışı bırak
            window.location.reload();

            if (response.newRemainingTime) {
                $('#remainingTime').text(response.newRemainingTime);
                $('#remainingTimeMessage').html('<div class="alert alert-info">Kalan süre: <span id="remainingTime">' + response.newRemainingTime + '</span></div>');
            }

            // Kalan süreyi baştan başlat
            updateRemainingTime();
            startTimer();
        } else {
            $('#resultMessage').html('<div class="alert alert-danger">' + response.message + '</div>');
        }
    });
});
