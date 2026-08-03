// Dashboard real-time state
const dashboardState = {
    products: [],
    benchmarks: [],
    refreshInterval: 2000
};

// Initialize dashboard
async function initializeDashboard() {
    renderNavbar();
    renderFooter();

    renderBreadcrumbContainer();

    updateCartBadge();
    updateCurrentTime();

    await loadDashboardData();

    setInterval(loadDashboardData, dashboardState.refreshInterval);
    setInterval(updateCurrentTime, 1000);
}

function renderBreadcrumbContainer() {
    const container = document.getElementById('breadcrumb-container');
    container.innerHTML = renderBreadcrumb([
        { label: 'Inicio', href: 'index.html' },
        { label: 'Dashboard' }
    ]);
}

// Update current time
function updateCurrentTime() {
    const now = new Date();
    document.getElementById('current-time').textContent = now.toLocaleTimeString('es-ES');
}

// Load all dashboard data
async function loadDashboardData() {
    try {
        const [stocks, benchmarks] = await Promise.all([
            fetch('/api/dashboard/stock').then(r => {
                if (!r.ok) throw new Error(`HTTP ${r.status}`);
                return r.json();
            }),
            fetch('/api/dashboard/benchmarks').then(r => {
                if (!r.ok) return [];
                return r.json();
            }).catch(() => [])
        ]);

        dashboardState.products = Array.isArray(stocks) ? stocks : [];
        dashboardState.benchmarks = Array.isArray(benchmarks) ? benchmarks : [];

        updateMetrics();
        updateSegmentation();
        updateBenchmarks();
        updateCategoryDistribution();
        updateProductsTable();
        updateLastRefreshTime();
    } catch (error) {
        console.error('Error cargando datos del dashboard:', error);
    }
}

// Update global metrics
function updateMetrics() {
    const stocks = dashboardState.products;
    if (!stocks.length) return;

    const totalInventory = stocks.reduce((sum, s) => sum + (s.initialStock || 0), 0);
    const totalAttempts = stocks.reduce((sum, s) => sum + (s.totalAttempts || 0), 0);
    const available = stocks.filter(s => s.currentBalance > 0).length;
    const soldOut = stocks.filter(s => s.currentBalance <= 0).length;

    document.getElementById('stat-total-inventory').textContent = totalInventory.toLocaleString();
    document.getElementById('stat-total-attempts').textContent = totalAttempts.toLocaleString();
    document.getElementById('stat-available-products').textContent = available;
    document.getElementById('stat-sold-out').textContent = soldOut;
}

// Update segmentation statistics
function updateSegmentation() {
    const totalAttempts = dashboardState.products.reduce((sum, s) => sum + (s.totalAttempts || 0), 0);
    if (totalAttempts === 0) return;

    document.getElementById('stat-high-demand').textContent = Math.round(totalAttempts * 0.80).toLocaleString();
    document.getElementById('stat-mid-demand').textContent = Math.round(totalAttempts * 0.15).toLocaleString();
    document.getElementById('stat-low-demand').textContent = Math.round(totalAttempts * 0.05).toLocaleString();
}

// Update benchmarks statistics
function updateBenchmarks() {
    if (!dashboardState.benchmarks.length) {
        document.getElementById('stat-events-generated').textContent = '0';
        document.getElementById('stat-events-per-second').textContent = '0';
        document.getElementById('stat-elapsed-seconds').textContent = '0';
        return;
    }

    const latest = dashboardState.benchmarks[0];

    document.getElementById('stat-events-generated').textContent = (latest.total_events_generated || 0).toLocaleString();
    document.getElementById('stat-events-per-second').textContent = (latest.events_per_second || 0).toFixed(2);
    document.getElementById('stat-elapsed-seconds').textContent = (latest.elapsed_seconds || 0).toFixed(2);
}

// Update category distribution
function updateCategoryDistribution() {
    const byCategory = {};

    dashboardState.products.forEach(stock => {
        const category = stock.categoryName || 'Sin categoría';
        byCategory[category] = (byCategory[category] || 0) + (stock.totalAttempts || 0);
    });

    const totalAttempts = Object.values(byCategory).reduce((a, b) => a + b, 0);
    const html = Object.entries(byCategory)
        .sort((a, b) => b[1] - a[1])
        .map(([category, attempts]) => {
            const percentage = totalAttempts > 0 ? ((attempts / totalAttempts) * 100).toFixed(1) : 0;
            return `
                <div>
                    <div class="flex justify-between mb-1">
                        <span class="text-sm font-medium text-gray-700">${category}</span>
                        <span class="text-sm font-bold text-gray-900">${percentage}% (${attempts.toLocaleString()})</span>
                    </div>
                    <div class="w-full bg-gray-300 rounded-full h-2">
                        <div class="bg-red-600 h-2 rounded-full" style="width: ${percentage}%"></div>
                    </div>
                </div>
            `;
        })
        .join('');

    document.getElementById('category-distribution').innerHTML = html || '<p class="text-gray-500 text-center py-4">Sin datos</p>';
}

// Update products table
function updateProductsTable() {
    const tbody = document.getElementById('products-tbody');
    const stocks = dashboardState.products;

    if (!stocks.length) {
        tbody.innerHTML = '<tr><td colspan="10" class="px-6 py-8 text-center text-gray-500">Sin datos</td></tr>';
        return;
    }

    const rows = stocks.map(stock => {
        const percentageAvailable = stock.initialStock > 0
            ? ((stock.currentBalance / stock.initialStock) * 100).toFixed(1)
            : 0;

        const statusBadge = stock.currentBalance > 0
            ? `<span class="px-3 py-1 bg-green-100 text-green-800 text-xs font-semibold rounded-full">Disponible</span>`
            : stock.currentBalance === 0
                ? `<span class="px-3 py-1 bg-yellow-100 text-yellow-800 text-xs font-semibold rounded-full">Agotado</span>`
                : `<span class="px-3 py-1 bg-red-100 text-red-800 text-xs font-semibold rounded-full">Sobreventa</span>`;

        return `
            <tr class="border-b border-gray-200 hover:bg-gray-50">
                <td class="px-6 py-4 font-medium text-gray-900">${stock.productName}</td>
                <td class="px-6 py-4 text-gray-600">${stock.productSku}</td>
                <td class="px-6 py-4 text-gray-600">${stock.categoryName}</td>
                <td class="px-6 py-4 text-right text-gray-900">${stock.initialStock.toLocaleString()}</td>
                <td class="px-6 py-4 text-right text-gray-900">${stock.totalAttempts.toLocaleString()}</td>
                <td class="px-6 py-4 text-right text-gray-900">${stock.soldUnits.toLocaleString()}</td>
                <td class="px-6 py-4 text-right text-gray-900">${stock.currentBalance.toLocaleString()}</td>
                <td class="px-6 py-4 text-center">
                    <span class="text-sm font-semibold text-gray-900">${percentageAvailable}%</span>
                </td>
                <td class="px-6 py-4 text-center">
                    <span class="text-sm font-semibold text-gray-900">${stock.conversionRate.toFixed(2)}%</span>
                </td>
                <td class="px-6 py-4 text-center">
                    ${statusBadge}
                </td>
            </tr>
        `;
    }).join('');

    tbody.innerHTML = rows;
}

function updateLastRefreshTime() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('es-ES');
    document.getElementById('last-refresh').textContent = timeString;
}

// Initialize on load
document.addEventListener('DOMContentLoaded', initializeDashboard);