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
    updateCartBadge();
    updateCurrentTime();
    await loadDashboardData();

    // Auto-refresh
    setInterval(loadDashboardData, dashboardState.refreshInterval);
    setInterval(updateCurrentTime, 1000);
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

        console.log('Stocks cargados:', dashboardState.products.length);
        console.log('Benchmarks cargados:', dashboardState.benchmarks.length);

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
        
        const conversionRate = stock.totalAttempts > 0
            ? ((stock.soldUnits / stock.totalAttempts) * 100).toFixed(1)
            : 0;

        let status = 'OK', statusClass = 'bg-green-100 text-green-700';
        if (stock.currentBalance < 0) {
            status = 'SOBREVENTA';
            statusClass = 'bg-red-100 text-red-700';
        } else if (stock.currentBalance === 0) {
            status = 'AGOTADO';
            statusClass = 'bg-orange-100 text-orange-700';
        } else if (stock.currentBalance <= 10) {
            status = 'BAJO';
            statusClass = 'bg-yellow-100 text-yellow-700';
        }

        const balanceClass = stock.currentBalance < 0 ? 'text-red-600 font-bold' : 
                            stock.currentBalance === 0 ? 'text-orange-600 font-bold' : 'text-green-600 font-bold';

        return `
            <tr class="hover:bg-gray-50 transition">
                <td class="px-6 py-3 text-sm font-medium text-gray-900">${stock.productName}</td>
                <td class="px-6 py-3 text-sm text-gray-600">${stock.productSku}</td>
                <td class="px-6 py-3 text-sm text-gray-600">${stock.categoryName}</td>
                <td class="px-6 py-3 text-right text-sm text-gray-900">${stock.initialStock.toLocaleString()}</td>
                <td class="px-6 py-3 text-right text-sm font-semibold text-gray-900">${(stock.totalAttempts || 0).toLocaleString()}</td>
                <td class="px-6 py-3 text-right text-sm text-gray-900">${(stock.soldUnits || 0).toLocaleString()}</td>
                <td class="px-6 py-3 text-right text-sm font-bold ${balanceClass}">${stock.currentBalance.toLocaleString()}</td>
                <td class="px-6 py-3 text-center text-sm text-gray-900">${percentageAvailable}%</td>
                <td class="px-6 py-3 text-center text-sm text-gray-900">${conversionRate}%</td>
                <td class="px-6 py-3 text-center">
                    <span class="px-3 py-1 rounded-full text-xs font-bold ${statusClass}">${status}</span>
                </td>
            </tr>
        `;
    }).join('');

    tbody.innerHTML = rows;
}

// Update last refresh time
function updateLastRefreshTime() {
    const now = new Date();
    document.getElementById('last-refresh').textContent = now.toLocaleTimeString('es-ES');
}

// Initialize on load
document.addEventListener('DOMContentLoaded', initializeDashboard);