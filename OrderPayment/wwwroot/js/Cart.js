// Helper function to format card number
function formatCardNumber(input) {
    let value = input.value.replace(/\D/g, ''); // Remove all non-digit characters
    value = value.replace(/(.{4})(?=.)/g, '$1 '); // Insert a space after every 4 digits
    input.value = value.substring(0, 19); // Limit input to 19 characters (16 digits + 3 spaces)

    toggleCardFlip(); // Check if card needs flipping
    validateInput(input); // Validate the card number
    checkPaymentButton(); // Enable/disable payment button
}

// Helper function to format expiry date
function formatExpiryDate(input) {
    let value = input.value.replace(/\D/g, ''); // Remove all non-digit characters
    value = value.replace(/^(\d{2})(\d{1,2})?/, (_, month, year) => (year ? `${month}/${year}` : month)); // Format as MM/YY
    input.value = value.substring(0, 5); // Limit input to 5 characters

    validateInput(input); // Validate expiry date
    checkPaymentButton(); // Enable/disable payment button
}

// Toggles the card flip effect based on input
function toggleCardFlip() {
    const cardNumber = document.getElementById('card-number').value.replace(/\D/g, '');
    const cardOwner = document.getElementById('card-owner').value.trim();

    const cardElement = document.getElementById('card');
    if (cardNumber.length === 16 && cardOwner.length > 0) {
        cardElement.classList.add('expanded'); // Add class for flipping effect
    } else {
        cardElement.classList.remove('expanded'); // Remove class
    }
}

// Enables or disables the payment button based on validation
function checkPaymentButton() {
    const cardNumber = document.getElementById('card-number').value.replace(/\D/g, '');
    const expiryDate = document.getElementById('expiry-date').value;
    const cvv = document.getElementById('cvv').value;
    const cardOwner = document.getElementById('card-owner').value.trim();

    const payButton = document.getElementById('pay-button');
    // Check if all inputs are valid
    if (cardNumber.length === 16 && expiryDate.length === 5 && cvv.length === 3 && cardOwner.length > 0) {
        payButton.disabled = false; // Enable the button
    } else {
        payButton.disabled = true; // Disable the button
    }
}

// Adds an error message below the input field
function showError(inputElement, message) {
    const parent = inputElement.parentElement;
    let errorMessage = parent.querySelector('.error-message');

    if (!errorMessage) {
        errorMessage = document.createElement('span');
        errorMessage.className = 'error-message';
        parent.appendChild(errorMessage);
    }
    errorMessage.textContent = message; // Display error message
}

// Removes an error message from the input field
function removeError(inputElement) {
    const parent = inputElement.parentElement;
    const errorMessage = parent.querySelector('.error-message');
    if (errorMessage) {
        errorMessage.remove(); // Remove error message element
    }
}

// Validates input fields
function validateInput(inputElement) {
    const value = inputElement.value.trim();
    const id = inputElement.id;

    // Validate card number
    if (id === 'card-number') {
        if (value.replace(/\D/g, '').length !== 16) {
            showError(inputElement, 'Card number must be 16 digits');
        } else {
            removeError(inputElement);
        }
    }
    // Validate expiry date
    else if (id === 'expiry-date') {
        const expiryRegex = /^(0[1-9]|1[0-2])\/\d{2}$/; // MM/YY format
        if (!expiryRegex.test(value)) {
            showError(inputElement, 'Enter a valid expiry date (MM/YY)');
        } else {
            removeError(inputElement);
        }
    }
    // Validate CVV
    else if (id === 'cvv') {
        if (value.length !== 3) {
            showError(inputElement, 'CVV must be 3 digits');
        } else {
            removeError(inputElement);
        }
    }
    // Validate card owner name
    else if (id === 'card-owner') {
        if (value.length === 0) {
            showError(inputElement, 'Card owner name is required');
        } else {
            removeError(inputElement);
        }
    }
}

// Event listeners for input formatting and validation
document.getElementById('card-number').addEventListener('input', formatCardNumber);
document.getElementById('expiry-date').addEventListener('input', formatExpiryDate);
document.getElementById('cvv').addEventListener('input', validateInput);
document.getElementById('cvv').addEventListener('input', checkPaymentButton);
document.getElementById('card-owner').addEventListener('input', validateInput);
document.getElementById('card-owner').addEventListener('input', checkPaymentButton);
