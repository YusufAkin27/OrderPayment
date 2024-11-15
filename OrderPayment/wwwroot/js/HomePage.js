$(document).ready(function () {
    $(".add-to-cart-btn").click(function () {
        var productId = $(this).data("product-id");

        // Send a POST request to the server
        $.post('/Home/AddToCart', { productId: productId }, function (response) {
            if (response.success) {
                alert(response.message);  // Show success message
                updateCartUI(response.cartTotal);
            } else {
                alert(response.message);  // Show error message
            }
        })
            .fail(function (xhr, status, error) {
                console.error("Error:", error);
                alert("Bir hata oluştu. Lütfen tekrar deneyin.");
            });
    });
});

// Function to update the cart UI with the total
function updateCartUI(cartTotal) {
    $("#cart-total").text(cartTotal);
}
