function addToCart(productId) {
    alert(`Ürün sepete eklendi: ${productId}`);
    // Add additional code here to handle adding the product to the cart
}

function changePage(pageNumber) {
    const url = new URL(window.location.href);
    url.searchParams.set('page', pageNumber);
    window.location.href = url.href;
}
