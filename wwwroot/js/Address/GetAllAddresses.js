// Modalı açma
function editAddress(id) {
    console.log(`Adres düzenleme işlemi başlatıldı. Adres ID: ${id}`);

    $.ajax({
        url: `/Address/GetAddressById?id=${id}`,
        type: "GET",
        success: function (data) {
            console.log("Adres verisi başarıyla alındı:", data);

            // Veriyi modal alanlarına yerleştir
            $("#editAddressId").val(data.id);
            $("#editAddressTitle").val(data.addressTitle);
            $("#editStreetAddress").val(data.streetAddress);
            $("#editNeighborhood").val(data.neighborhood);
            $("#editDistrict").val(data.district);
            $("#editCity").val(data.city);
            $("#editPhoneNumber").val(data.phoneNumber);
            $("#editDescription").val(data.addressDescription);


            // Modal'ı görünür hale getirmek için 'show' sınıfını ekleyin
            $("#editAddressModal").addClass("show");
            console.log("Modal açıldı ve veriler yerleştirildi.");
        },
        error: function (xhr, status, error) {
            console.error("Adres bilgileri yüklenirken bir hata oluştu:", error);
            alert("Adres bilgileri yüklenirken bir hata oluştu.");
        }
    });
}



// Adresi silme
function deleteAddress(addressId) {
    if (!confirm("Bu Adresi silmek istediğinizden emin misiniz?")) {
        return;
    }

    // AJAX DELETE isteği
    $.ajax({
        url: "/Address/DeleteAddress", // URL'i kontrol edin, silme işlemi için doğru olmalı
        type: "DELETE",
        data: { addressId: addressId }, // Veri gönderimi
        success: function (response) {
            // Adres başarıyla silindiğinde, adresi DOM'dan kaldır
            $('#address-' + addressId).fadeOut(400, function () {
                $(this).remove();
            });
            alert(response.message || 'Adres başarıyla silindi.');  // Sunucudan gelen mesajı göster
        },
        error: function (xhr, status, error) {
            // Sunucudan gelen hata mesajını kontrol et
            if (xhr.responseJSON && xhr.responseJSON.message) {
                alert('Adres silinemedi: ' + xhr.responseJSON.message);  // Hata mesajını göster
            } else {
                alert('Adres silinemedi: Bir hata oluştu. Lütfen tekrar deneyin.');  // Genel hata mesajı
            }
        }
    });
}



// Adresi güncelleme
// Adresi güncelleme
function submitAddressUpdate() {
    const addressData = {
        id: $("#editAddressId").val(),
        addressTitle: $("#editAddressTitle").val(),
        streetAddress: $("#editStreetAddress").val(),
        neighborhood: $("#editNeighborhood").val(),
        district: $("#editDistrict").val(),
        city: $("#editCity").val(),
        phoneNumber: $("#editPhoneNumber").val(),
        AddressDescription: document.getElementById("editDescription").value.trim(),
    };

    console.log("Adres güncelleme için gönderilen veri:", addressData);

    $.ajax({
        url: "/Address/UpdateAddress",
        type: "PATCH",
        contentType: "application/json",
        data: JSON.stringify(addressData),
        success: function () {
            console.log("Adres başarıyla güncellendi.");
            alert("Adres başarıyla güncellendi.");
            $("#editAddressModal").fadeOut();
            location.reload(); // Listeyi güncellemek için sayfayı yenile
        },
        error: function (xhr, status, error) {
            console.error("Adres güncellenirken bir hata oluştu:", error);
            if (xhr.responseJSON && xhr.responseJSON.message) {
                alert(xhr.responseJSON.message);  // Sunucudan gelen hata mesajını göster
            } else {
                alert("Adres güncellenirken bir hata oluştu.");
            }
        }
    });
}

