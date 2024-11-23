document.addEventListener("DOMContentLoaded", function () {
    const editButton = document.getElementById("edit-profile-button");
    const deleteButton = document.getElementById("delete-profile-button");
    const statusMessageDiv = document.getElementById("status-message");  // Message container for success/error

    // Kullanıcıya ait mevcut verileri saklayalım (form yüklenmeden önce)
    const firstNameField = document.getElementById("firstName");
    const lastNameField = document.getElementById("lastName");
    const passwordField = document.getElementById("password");
    const phoneNumberField = document.getElementById("phoneNumber");

    // Mevcut verileri kaydediyoruz (disabled olmayan haldeki veriler)
    const currentFirstName = firstNameField.value.trim();
    const currentLastName = lastNameField.value.trim();
    const currentPhoneNumber = phoneNumberField.value.trim();
    const currentPassword = passwordField.value.trim();

    // Düzenleme butonuna tıklama olayını ekle
    editButton.addEventListener("click", function () {
        // Alanları aktif yap
        firstNameField.disabled = false;
        lastNameField.disabled = false;
        passwordField.disabled = false;
        phoneNumberField.disabled = false;

        // Düzenle butonunun metnini değiştir
        editButton.innerHTML = '<i class="fas fa-save"></i> Profili Güncelle';

        // Profili güncelle butonuna tıkladığında, verileri güncelleme işlemi yapılabilir
        editButton.setAttribute("href", "javascript:void(0);"); // Linki etkinleştirme

        // Butonun tıklama olayını yalnızca bir kez çalıştır
        editButton.removeEventListener("click", arguments.callee);  // Önceki tıklama işleyicisini kaldır

        // Yeni buton işleyicisini ekleyelim
        editButton.addEventListener("click", function () {
            // Alanlardan veriyi al
            const firstName = firstNameField.value.trim();
            const lastName = lastNameField.value.trim();
            const password = passwordField.value.trim();
            const phoneNumber = phoneNumberField.value.trim();

            // Güncellenen veriyi hazırlıyoruz
            let updatedData = {};

            // Değişen alanları güncelleyelim, değişmeyenleri ise eski haliyle tutalım
            updatedData.firstName = firstName || currentFirstName;
            updatedData.lastName = lastName || currentLastName;
            updatedData.phoneNumber = phoneNumber || currentPhoneNumber;
            updatedData.password = password || currentPassword;

            // Güncellenen veriyi kontrol edelim
            let changesList = [];  // Değişikliklerin listeleneceği dizi

            if (firstName !== currentFirstName) {
                changesList.push(`Ad: ${firstName}`);
            }
            if (lastName !== currentLastName) {
                changesList.push(`Soyad: ${lastName}`);
            }
            if (phoneNumber !== currentPhoneNumber) {
                changesList.push(`Telefon: ${phoneNumber}`);
            }
            if (password !== currentPassword) {
                changesList.push(`Şifre: Güncellendi`);
            }

            // Eğer hiçbir değişiklik yapılmadıysa, işlem yapılmasın
            if (changesList.length === 0) {
                displayMessage("Hiçbir değişiklik yapılmadı.", "error");
                return;
            }

            // Değişen alanları konsola yazdır
            console.log("Değiştirilen Alanlar:", changesList);
            console.log("JSON Formatında değişen alanlar :", JSON.stringify(updatedData));

            // JSON formatında sunucuya gönder
            fetch('/User/EditProfile', {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updatedData)  // Güncellenen verileri JSON formatında gönderiyoruz
            })
                .then(response => {
                    if (!response.ok) {
                        return response.json().then(errorData => {
                            // Hata mesajları varsa, bunları yakalayalım
                            throw new Error(errorData.error || 'Bilinmeyen bir hata oluştu');
                        });
                    }
                    return response.json();  // JSON dönecek
                })
                .then(data => {
                    if (data.message) {
                        displayMessage(data.message, "success");  // Sunucudan gelen başarılı mesajı göster
                        window.location.reload();  // Sayfayı yenileyin
                    } else {
                        displayMessage("Güncellenirken bir hata oluştu.", "error");
                    }
                })
                .catch(error => {
                    console.error("Error:", error);
                    displayMessage("Bir hata oluştu. Lütfen tekrar deneyin.\n" + error.message, "error");
                });
        });
    });

    // Hesabımı sil butonuna tıklama olayı
    deleteButton.addEventListener("click", function () {
        // Kullanıcıya onay mesajı göster
        const confirmation = confirm("Hesabınızı silmek üzeresiniz. Bu işlem geri alınamaz. Emin misiniz?");

        if (confirmation) {
            // Silme işlemi için GET isteği gönder
            fetch('/User/DeleteUser', {
                method: 'GET', // HTTP GET yöntemi
            })
                .then(response => {
                    if (!response.ok) {
                        displayMessage("Hesabınızı silerken bir hata oluştu.", "error");
                        return;
                    }
                    displayMessage("Hesabınız başarıyla silindi.", "success");
                    window.location.href = '/Auth/Login'; // Hesap silindikten sonra login sayfasına yönlendir
                })
                .catch(error => {
                    displayMessage("Bir hata oluştu. Lütfen tekrar deneyin.", "error");
                });
        }
    });


    // Mesaj gösterme fonksiyonu
    function displayMessage(message, type) {
        statusMessageDiv.textContent = message;
        statusMessageDiv.className = `status-message ${type}`;
        statusMessageDiv.style.display = 'block';
        setTimeout(() => {
            statusMessageDiv.style.display = 'none';
        }, 5000);
    }
});
