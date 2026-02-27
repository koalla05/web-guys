import { useState, useMemo } from 'react';
import { DashboardSidebar } from './components/dashboard-sidebar';
import { DashboardHeader } from './components/dashboard-header';
import { CSVUploadCard } from './components/csv-upload-card';
import { CreateOrderCard } from './components/create-order-card';
import { OrdersTable, Order } from './components/orders-table';
import { OrdersFilters, OrderFilters } from './components/orders-filters';
import { OrdersPagination } from './components/orders-pagination';
import { TaxBreakdownModal } from './components/tax-breakdown-modal';
import { StatsCards } from './components/stats-cards';
import { PageHeader } from './components/page-header';
import { toast } from 'sonner';
import { Toaster } from './components/ui/sonner';

// Mock data generator
const generateMockOrders = (): Order[] => {
  const counties = ['New York', 'Kings', 'Queens', 'Nassau', 'Suffolk', 'Westchester', 'Erie', 'Monroe'];
  const orders: Order[] = [];

  for (let i = 1; i <= 42; i++) {
    const subtotal = parseFloat((Math.random() * 300 + 50).toFixed(2));
    const compositeTaxRate = parseFloat((Math.random() * 4 + 7).toFixed(2));
    const taxAmount = parseFloat((subtotal * compositeTaxRate / 100).toFixed(2));
    const totalAmount = parseFloat((subtotal + taxAmount).toFixed(2));

    orders.push({
      id: `ORD-${String(i).padStart(5, '0')}`,
      timestamp: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
      latitude: parseFloat((40.5 + Math.random() * 2).toFixed(4)),
      longitude: parseFloat((-74.5 + Math.random() * 2).toFixed(4)),
      subtotal,
      compositeTaxRate,
      taxAmount,
      totalAmount,
      county: counties[Math.floor(Math.random() * counties.length)],
    });
  }

  return orders.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
};

function App() {
  const [orders, setOrders] = useState<Order[]>(generateMockOrders());
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);
  const [filters, setFilters] = useState<OrderFilters>({});

  // Filter orders based on active filters
  const filteredOrders = useMemo(() => {
    return orders.filter((order) => {
      // Search by Order ID
      if (filters.searchQuery && !order.id.toLowerCase().includes(filters.searchQuery.toLowerCase())) {
        return false;
      }

      // Date range filter
      if (filters.dateFrom) {
        const orderDate = new Date(order.timestamp);
        const fromDate = new Date(filters.dateFrom);
        fromDate.setHours(0, 0, 0, 0);
        if (orderDate < fromDate) return false;
      }

      if (filters.dateTo) {
        const orderDate = new Date(order.timestamp);
        const toDate = new Date(filters.dateTo);
        toDate.setHours(23, 59, 59, 999);
        if (orderDate > toDate) return false;
      }

      // Tax rate range filter
      if (filters.taxRateMin && order.compositeTaxRate < parseFloat(filters.taxRateMin)) {
        return false;
      }

      if (filters.taxRateMax && order.compositeTaxRate > parseFloat(filters.taxRateMax)) {
        return false;
      }

      // Amount range filter (total amount)
      if (filters.amountMin && order.totalAmount < parseFloat(filters.amountMin)) {
        return false;
      }

      if (filters.amountMax && order.totalAmount > parseFloat(filters.amountMax)) {
        return false;
      }

      // County filter
      if (filters.county && order.county !== filters.county) {
        return false;
      }

      return true;
    });
  }, [orders, filters]);

  // Reset to page 1 whenever filters change
  const handleFilterChange = (newFilters: OrderFilters) => {
    setFilters(newFilters);
    setCurrentPage(1);
  };

  const handleClearFilters = () => {
    setFilters({});
    setCurrentPage(1);
  };

  const handleCreateOrder = (orderData: {
    latitude: number;
    longitude: number;
    subtotal: number;
    timestamp: string;
  }) => {
    const subtotal = orderData.subtotal;
    const compositeTaxRate = parseFloat((Math.random() * 4 + 7).toFixed(2));
    const taxAmount = parseFloat((subtotal * compositeTaxRate / 100).toFixed(2));
    const totalAmount = parseFloat((subtotal + taxAmount).toFixed(2));

    const newOrder: Order = {
      id: `ORD-${String(orders.length + 1).padStart(5, '0')}`,
      timestamp: orderData.timestamp,
      latitude: orderData.latitude,
      longitude: orderData.longitude,
      subtotal,
      compositeTaxRate,
      taxAmount,
      totalAmount,
      county: 'New York',
    };

    setOrders([newOrder, ...orders]);
    toast.success('Order created successfully!');
  };

  const handleViewBreakdown = (orderId: string) => {
    setSelectedOrderId(orderId);
  };

  const selectedOrder = orders.find((o) => o.id === selectedOrderId);
  const breakdown = selectedOrder
    ? {
        orderId: selectedOrder.id,
        stateRate: 4.0,
        countyRate: parseFloat((selectedOrder.compositeTaxRate - 5.5).toFixed(2)),
        cityRate: 1.0,
        specialRate: 0.5,
        compositeTaxRate: selectedOrder.compositeTaxRate,
        taxAmount: selectedOrder.taxAmount,
        subtotal: selectedOrder.subtotal,
        totalAmount: selectedOrder.totalAmount,
        state: 'New York',
        county: `${selectedOrder.county} County`,
        city: 'New York City',
        specialDistrict: 'Metropolitan Transport',
      }
    : null;

  const totalPages = Math.ceil(filteredOrders.length / pageSize);
  const paginatedOrders = filteredOrders.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  // Calculate stats from filtered orders
  const totalRevenue = filteredOrders.reduce((sum, order) => sum + order.totalAmount, 0);
  const totalTax = filteredOrders.reduce((sum, order) => sum + order.taxAmount, 0);
  const averageTaxRate = filteredOrders.length > 0 
    ? filteredOrders.reduce((sum, order) => sum + order.compositeTaxRate, 0) / filteredOrders.length
    : 0;

  return (
    <div className="flex min-h-screen bg-background wellness-gradient">
      <DashboardSidebar />
      
      <div className="flex-1 flex flex-col">
        <DashboardHeader />
        
        <main className="flex-1 p-8">
          {/* Page Header */}
          <PageHeader />

          {/* Stats Cards */}
          <StatsCards
            totalOrders={orders.length}
            totalRevenue={totalRevenue}
            totalTax={totalTax}
            averageTaxRate={averageTaxRate}
          />

          {/* Cards Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            <CSVUploadCard />
            <CreateOrderCard onCreateOrder={handleCreateOrder} />
          </div>

          {/* Filters */}
          <div className="mb-6">
            <OrdersFilters 
              filters={filters}
              onFilterChange={handleFilterChange} 
              onClearFilters={handleClearFilters}
              resultsCount={filteredOrders.length}
            />
          </div>

          {/* Orders Table Section */}
          <div className="bg-white rounded-xl border border-border p-6 shadow-sm">
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-foreground mb-1">Recent Orders</h2>
              <p className="text-sm text-muted-foreground">
                View and manage all drone delivery orders with tax calculations
              </p>
            </div>

            <OrdersTable
              orders={paginatedOrders}
              onViewBreakdown={handleViewBreakdown}
              isLoading={false}
            />

            {filteredOrders.length > 0 && (
              <OrdersPagination
                currentPage={currentPage}
                totalPages={totalPages}
                pageSize={pageSize}
                totalItems={filteredOrders.length}
                onPageChange={setCurrentPage}
                onPageSizeChange={(size) => {
                  setPageSize(size);
                  setCurrentPage(1);
                }}
              />
            )}
          </div>
        </main>
      </div>

      {/* Tax Breakdown Modal */}
      <TaxBreakdownModal
        open={!!selectedOrderId}
        onClose={() => setSelectedOrderId(null)}
        breakdown={breakdown}
      />

      {/* Toaster */}
      <Toaster />
    </div>
  );
}

export default App;