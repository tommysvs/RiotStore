// Dashboard real-time state
const dashboardState = {
    statistics: null,
    products: [],
    refreshInterval: 2000,
    autoRefreshEnabled: true
};

// Initialize dashboard
async function initializeDashboard() {
    updateCurrentTime();
    setInterval(updateCurrentTime, 1000);

    // First load
    await loadDashboardData();

    // Auto-refresh every X seconds
    setInterval(loadDashboardData, dashboardState.refreshInterval);
}

// Update current time
function updateCurrentTime() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('es-ES', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
    document.getElementById('current-time').textContent = timeString;
}

// Load all dashboard data
async function loadDashboardData() {
    try {
        const [statsResponse, stockResponse] = await Promise.all([
            fetch(`${API_BASE_URL}/dashboard/statistics`),
            fetch(`${API_BASE_URL}/dashboard/all-stock`)
        ]);

        if (!statsResponse.ok || !stockResponse.ok) {
            console.error('Error fetching data');
            return;
        }

        dashboardState.statistics = await statsResponse.json();
        dashboardState.products = await stockResponse.json();

        updateGlobalStatistics();
        updateProductsTable();
        updateLastRefreshTime();
    } catch (error) {
        console.error('Error loading dashboard data:', error);
    }
}

// Update global statistics cards
function updateGlobalStatistics() {
    const stats = dashboardState.statistics;

    document.getElementById('stat-total-inventory').textContent =
        stats.totalInventory.toLocaleString('es-ES');

    document.getElementById('stat-total-attempts').textContent =
        stats.totalAttempts.toLocaleString('es-ES');

    document.getElementById('stat-conversion-rate').textContent =
        `${stats.globalConversionRate.toFixed(2)}%`;

    document.getElementById('stat-overselling').textContent =
        `${stats.globalOverselling.toFixed(2)}%`;

    document.getElementById('stat-available-products').textContent =
        stats.availableProducts;

    document.getElementById('stat-low-stock').textContent =
        stats.lowStockProducts;

    document.getElementById('stat-exhausted').textContent =
        stats.exhaustedProducts;

    document.getElementById('stat-oversold-products').textContent =
        stats.oversoldProducts;
}

// Update products table
function updateProductsTable() {
    const tbody = document.getElementById('products-tbody');

    if (!dashboardState.products || dashboardState.products.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="9" class="px-6 py-12 text-center text-gray-500">
                    No hay productos registrados
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = dashboardState.products.map(product => {
        const isOversold = product.isOversold;
        const statusClass = isOversold
            ? 'status-oversold'
            : product.currentBalance === 0
                ? 'status-exhausted'
                : product.currentBalance <= 10
                    ? 'status-low'
                    : 'status-available';

        const statusText = isOversold
            ? 'SOBREVENTA'
            : product.currentBalance === 0
                ? 'Agotado'
                : product.currentBalance <= 10
                    ? 'Stock Bajo'
                    : 'Disponible';

        const percentageClass = product.currentBalance < 0
            ? 'text-red-600 font-bold'
            : 'text-gray-800';

        return `
            <tr class="hover:bg-gray-50 transition ${isOversold ? 'bg-red-50' : ''}">
                <td class="px-6 py-4">
                    <div class="font-semibold text-gray-900">${product.productName}</div>
                </td>
                <td class="px-6 py-4 text-gray-600">${product.productSku}</td>
                <td class="px-6 py-4 text-right text-gray-800 font-semibold">
                    ${product.initialStock.toLocaleString('es-ES')}
                </td>
                <td class="px-6 py-4 text-right text-purple-600 font-semibold">
                    ${product.totalAttempts.toLocaleString('es-ES')}
                </td>
                <td class="px-6 py-4 text-right text-green-600 font-semibold">
                    ${product.soldUnits.toLocaleString('es-ES')}
                </td>
                <td class="px-6 py-4 text-right font-bold ${percentageClass}">
                    ${product.currentBalance.toLocaleString('es-ES')}
                </td>
                <td class="px-6 py-4 text-center">
                    <div class="w-full bg-gray-200 rounded-full h-2">
                        <div class="bg-gradient-to-r from-green-500 to-blue-500 h-2 rounded-full" 
                            style="width: ${Math.max(0, Math.min(100, product.percentageRemaining))}%"></div>
                    </div>
                    <span class="text-xs text-gray-600">${product.percentageRemaining.toFixed(1)}%</span>
                </td>
                <td class="px-6 py-4 text-center text-sm font-semibold">
                    ${product.conversionRate.toFixed(2)}%
                </td>
                <td class="px-6 py-4 text-center">
                    <span class="status-badge ${statusClass}">
                        ${statusText}
                    </span>
                </td>
            </tr>
        `;
    }).join('');
}

// Update last refresh time
function updateLastRefreshTime() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('es-ES', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
    document.getElementById('last-refresh').textContent = timeString;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', initializeDashboard);