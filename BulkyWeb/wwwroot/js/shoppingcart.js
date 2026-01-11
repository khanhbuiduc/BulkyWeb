// Shopping Cart JavaScript
$(document).ready(function () {
    // Confirmation before removing item
    $('.btn-remove-item').on('click', function (e) {
        if (!confirm('Are you sure you want to remove this item from your cart?')) {
            e.preventDefault();
        }
    });

    // Update cart badge count
    updateCartCount();
});

function updateCartCount() {
    // This function can be enhanced to fetch cart count via AJAX
    // and update a badge in the navigation bar
    console.log('Cart count updated');
}

// Optional: Add animation when quantity changes
function animateQuantityChange(element) {
    $(element).addClass('quantity-changed');
    setTimeout(function () {
        $(element).removeClass('quantity-changed');
    }, 300);
}
