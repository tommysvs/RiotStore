const CART_KEY = 'riotstore_cart';
const API_BASE_URL = '/api';

// Retrieves the cart from localStorage
function getCart() {
    const cart = localStorage.getItem(CART_KEY);
    return cart ? JSON.parse(cart) : [];
}

// Saves the cart to localStorage
function saveCart(cart) {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
}

// Adds a product to the cart
function addToCart(product, quantity = 1) {
    const cart = getCart();
    const existingItem = cart.find(item => item.product_id === product.product_id);

    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        cart.push({
            product_id: product.product_id,
            sku: product.sku,
            name: product.name,
            price: product.price,
            image_url: product.image_url,
            quantity: quantity
        });
    }

    saveCart(cart);
    updateCartBadge();
    showNotification(`Producto añadido al carrito`, 'success');
}

// Removes a product from the cart
function removeFromCart(index) {
    const cart = getCart();
    cart.splice(index, 1);
    saveCart(cart);
    updateCartBadge();
}

// Updates the quantity of a product in the cart
function updateQuantity(index, quantity) {
    const cart = getCart();
    if (quantity <= 0) {
        removeFromCart(index);
    } else {
        cart[index].quantity = quantity;
        saveCart(cart);
        updateCartBadge();
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
        badge.textContent = cart.reduce((sum, item) => sum + item.quantity, 0);
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
    if (stock > 10) return 'En Stock';
    if (stock > 0) return 'Pocas Unidades';
    return 'Agotado';
}

// Returns CSS classes for the stock indicator
function getStockClass(stock) {
    if (stock > 10) return 'bg-green-100 text-green-800';
    if (stock > 0) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
}

// Fetch products from API
async function fetchProducts() {
    try {
        const response = await fetch(`${API_BASE_URL}/products`);
        return await response.json();
    } catch (error) {
        console.error('Error fetching products:', error);
        return [];
    }
}

// Fetch categories from API
async function fetchCategories() {
    try {
        const response = await fetch(`${API_BASE_URL}/products/categories`);
        return await response.json();
    } catch (error) {
        console.error('Error fetching categories:', error);
        return [];
    }
}

// Fetch products by category
async function fetchProductsByCategory(categoryId) {
    try {
        const response = await fetch(`${API_BASE_URL}/products/category/${categoryId}`);
        return await response.json();
    } catch (error) {
        console.error('Error fetching products:', error);
        return [];
    }
}