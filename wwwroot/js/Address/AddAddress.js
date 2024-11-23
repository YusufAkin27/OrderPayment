document.getElementById("addAddressForm").addEventListener("submit", async (event) => {
    event.preventDefault(); // Formun varsayılan gönderimini engelle

    // Formdan verileri al
    const formData = {
        AddressTitle: document.getElementById("AddressTitle").value.trim(),
        StreetAddress: document.getElementById("StreetAddress").value.trim(),
        Neighborhood: document.getElementById("Neighborhood").value.trim(),
        District: document.getElementById("District").value.trim(),
        City: document.getElementById("City").value.trim(),
        AddressDescription: document.getElementById("AddressDescription").value.trim(),
        PhoneNumber: document.getElementById("PhoneNumber").value.trim(),
    };

    // Hata mesajlarını temizle
    document.querySelectorAll('.text-danger').forEach(el => el.textContent = '');

    let isValid = true;

    // Boş alanları kontrol et ve uygun hata mesajlarını göster
    if (!formData.AddressTitle) {
        document.getElementById("AddressTitleError").textContent = "Adres başlığı zorunludur!";
        isValid = false;
    }
    if (!formData.StreetAddress) {
        document.getElementById("StreetAddressError").textContent = "Sokak adresi zorunludur!";
        isValid = false;
    }
    if (!formData.Neighborhood) {
        document.getElementById("NeighborhoodError").textContent = "Mahalle adı zorunludur!";
        isValid = false;
    }
    if (!formData.District) {
        document.getElementById("DistrictError").textContent = "İlçe adı zorunludur!";
        isValid = false;
    }
    if (!formData.City) {
        document.getElementById("CityError").textContent = "Şehir adı zorunludur!";
        isValid = false;
    }
    if (!formData.PhoneNumber || !/^\d{10}$/.test(formData.PhoneNumber)) {
        document.getElementById("PhoneNumberError").textContent = "Geçerli bir telefon numarası giriniz!";
        isValid = false;
    }

    if (!isValid) return;

    try {
        // AJAX ile POST isteği gönder
        const response = await fetch('/Address/AddAddress', {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(formData), // JSON olarak gönder
        });

        // Gelen cevabı kontrol et
        if (response.ok) {
            alert("Adres başarıyla eklendi!");
            window.location.href = "/Address/GetAllAddresses"; // Başarıyla adres eklendiyse yönlendir
        } else {
            // Eğer JSON değilse, ham yanıtı al ve hatayı kontrol et
            const textData = await response.text();

            try {
                const errorData = JSON.parse(textData);
                alert(errorData.message || "Bir hata oluştu!");
            } catch (jsonError) {
                alert("Bir hata oluştu: " + textData);  // JSON formatında değilse, ham veriyi göster
            }
        }
    } catch (error) {
        console.error("Hata:", error);
        alert("Bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
    }
});
