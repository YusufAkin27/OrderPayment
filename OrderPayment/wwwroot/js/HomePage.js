$(document).ready(function () {
    $(".add-to-cart-btn").click(function () {
        var productId = $(this).data("product-id"); // Get the product ID from the button

        // Send a POST request to the server
        $.post('/Home/AddToCart', { productId: productId }, function (response) {
            // Handle the response from the server
            if (response.success) {
                alert(response.message);  // Show success message (e.g., product added to cart)
                // Update cart total UI if the response provides a new total
                updateCartUI(response.cartTotal);
            } else {
                alert(response.message);  // Show error message if not successful
            }
        })
            .fail(function (xhr, status, error) {
                // Handle network or other errors
                console.error("Error:", error);
                alert("Bir hata oluştu. Lütfen tekrar deneyin.");
            });
    });
});

// Function to update the cart UI with the total
function updateCartUI(cartTotal) {
    $("#cart-total").text(cartTotal);  // Update the cart total displayed in the UI
}
