// Shared utilities for the entire application
const CART_KEY = 'riotstore_cart';

// Retrieves the cart from localStorage
function getCart() {
    const cart = localStorage.getItem(CART_KEY);
    return cart ? JSON.parse(cart) : [];
}

// Saves the cart to localStorage
function saveCart(cart) {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
    updateCartBadge();
}

// Adds a product to the cart
function addToCart(product, quantity = 1) {
    const cart = getCart();
    const existingItem = cart.find(item => item.id === product.id);

    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        cart.push({
            ...product,
            quantity: quantity
        });
    }

    saveCart(cart);
}

// Removes a product from the cart
function removeFromCart(index) {
    const cart = getCart();
    cart.splice(index, 1);
    saveCart(cart);
}

// Updates the quantity of a product in the cart
function updateQuantity(index, quantity) {
    const cart = getCart();
    if (cart[index]) {
        cart[index].quantity = Math.max(1, quantity);
        saveCart(cart);
    }
}

// Clears the cart
function clearCart() {
    localStorage.removeItem(CART_KEY);
    updateCartBadge();
}

// Updates the cart badge in the navbar
function updateCartBadge() {
    const cart = getCart();
    const badge = document.getElementById('cart-badge');
    if (badge) {
        const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
        badge.textContent = totalItems;
        badge.style.display = totalItems > 0 ? 'flex' : 'none';
    }
}

// Formats a number as USD price
function formatPrice(price) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(price);
}

// Returns the stock label based on quantity
function getStockLabel(stock) {
    if (stock <= 0) return 'Out of Stock / Oversold';
    if (stock <= 5) return `Only ${stock} left`;
    return 'Available';
}

// Returns CSS classes for the stock indicator
function getStockClass(stock) {
    let baseClasses = 'text-center py-2 px-4 border-2 uppercase font-semibold text-sm tracking-wider';

    if (stock <= 0) {
        return baseClasses + ' border-red-600 text-red-400 bg-red-950';
    } else if (stock <= 5) {
        return baseClasses + ' border-yellow-600 text-yellow-400 bg-yellow-950';
    } else {
        return baseClasses + ' border-green-600 text-green-400 bg-green-950';
    }
}