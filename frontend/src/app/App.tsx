import { useState, useEffect, useCallback } from 'react';
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
import * as api from './lib/api';

// Map API order to display order (tax rate as % for display)
function toDisplayOrder(o: api.Order): Order {
  return {
    id: o.id,
    timestamp: o.timestamp,
    latitude: o.latitude,
    longitude: o.longitude,
    subtotal: o.subtotal,
    compositeTaxRate: o.composite_tax_rate * 100,
    taxAmount: o.tax_amount,
    totalAmount: o.total_amount,
    county: o.county ?? '—',
  };
}

function App() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);
  const [orderDetail, setOrderDetail] = useState<api.OrderDetail | null>(null);
  const [filters, setFilters] = useState<OrderFilters>({});
  const [isLoading, setIsLoading] = useState(true);
  const [isDetailLoading, setIsDetailLoading] = useState(false);

  const fetchOrders = useCallback(async () => {
    setIsLoading(true);
    try {
      const params: Parameters<typeof api.getOrders>[0] = {
        page: currentPage,
        pageSize,
        orderIdSearch: filters.searchQuery || undefined,
        fromDate: filters.dateFrom || undefined,
        toDate: filters.dateTo || undefined,
        county: filters.county || undefined,
        minAmount: filters.amountMin ? parseFloat(filters.amountMin) : undefined,
        maxAmount: filters.amountMax ? parseFloat(filters.amountMax) : undefined,
        minTaxRate: filters.taxRateMin ? parseFloat(filters.taxRateMin) / 100 : undefined,
        maxTaxRate: filters.taxRateMax ? parseFloat(filters.taxRateMax) / 100 : undefined,
      };
      const result = await api.getOrders(params);
      setOrders(result.items.map(toDisplayOrder));
      setTotalCount(result.total_count);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to load orders');
      setOrders([]);
      setTotalCount(0);
    } finally {
      setIsLoading(false);
    }
  }, [currentPage, pageSize, filters]);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  useEffect(() => {
    if (!selectedOrderId) {
      setOrderDetail(null);
      return;
    }
    setIsDetailLoading(true);
    api.getOrderById(selectedOrderId).then((detail) => {
      setOrderDetail(detail ?? null);
      setIsDetailLoading(false);
    });
  }, [selectedOrderId]);

  const handleFilterChange = (newFilters: OrderFilters) => {
    setFilters(newFilters);
    setCurrentPage(1);
  };

  const handleClearFilters = () => {
    setFilters({});
    setCurrentPage(1);
  };

  const handleCreateOrder = async (orderData: {
    latitude: number;
    longitude: number;
    subtotal: number;
    timestamp: string;
  }) => {
    try {
      await api.createOrder({
        latitude: orderData.latitude,
        longitude: orderData.longitude,
        subtotal: orderData.subtotal,
        timestamp: orderData.timestamp ? new Date(orderData.timestamp).toISOString() : undefined,
      });
      toast.success('Order created successfully!');
      fetchOrders();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to create order');
    }
  };

  const handleCsvImport = async (file: File) => {
    try {
      const result = await api.importOrders(file);
      if (result.imported_count > 0) {
        toast.success(`Imported ${result.imported_count} order(s)`);
        if (result.failed_count > 0) {
          toast.warning(`${result.failed_count} row(s) failed. Check errors.`);
        }
        fetchOrders();
      } else {
        toast.error(result.errors?.[0] || 'No orders imported');
      }
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to import CSV');
    }
  };

  const handleViewBreakdown = (orderId: string) => {
    setSelectedOrderId(orderId);
  };

  const breakdown = orderDetail
    ? {
        orderId: orderDetail.id,
        stateRate: orderDetail.tax_rate_breakdown.state_rate * 100,
        countyRate: orderDetail.tax_rate_breakdown.county_rate * 100,
        cityRate: orderDetail.tax_rate_breakdown.city_rate * 100,
        specialRate: orderDetail.tax_rate_breakdown.special_rates * 100,
        compositeTaxRate: orderDetail.tax_rate_breakdown.composite_tax_rate * 100,
        taxAmount: orderDetail.amount_calculation.tax_amount,
        subtotal: orderDetail.amount_calculation.subtotal,
        totalAmount: orderDetail.amount_calculation.total_amount,
        state: orderDetail.applied_jurisdictions.state ?? '—',
        county: orderDetail.applied_jurisdictions.county ?? '—',
        city: orderDetail.applied_jurisdictions.city ?? '—',
        specialDistrict: orderDetail.applied_jurisdictions.special ?? undefined,
      }
    : null;

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const totalRevenue = orders.reduce((sum, o) => sum + o.totalAmount, 0);
  const totalTax = orders.reduce((sum, o) => sum + o.taxAmount, 0);
  const averageTaxRate =
    orders.length > 0 ? orders.reduce((sum, o) => sum + o.compositeTaxRate, 0) / orders.length : 0;

  return (
    <div className="flex min-h-screen bg-background wellness-gradient">
      <DashboardSidebar />

      <div className="flex-1 flex flex-col">
        <DashboardHeader />

        <main className="flex-1 p-8">
          <PageHeader />

          <StatsCards
            totalOrders={totalCount}
            totalRevenue={totalRevenue}
            totalTax={totalTax}
            averageTaxRate={averageTaxRate}
          />

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            <CSVUploadCard onImport={handleCsvImport} />
            <CreateOrderCard onCreateOrder={handleCreateOrder} />
          </div>

          <div className="mb-6">
            <OrdersFilters
              filters={filters}
              onFilterChange={handleFilterChange}
              onClearFilters={handleClearFilters}
              resultsCount={totalCount}
            />
          </div>

          <div className="bg-white rounded-xl border border-border p-6 shadow-sm">
            <div className="mb-6">
              <h2 className="text-xl font-semibold text-foreground mb-1">Recent Orders</h2>
              <p className="text-sm text-muted-foreground">
                View and manage all drone delivery orders with tax calculations
              </p>
            </div>

            <OrdersTable
              orders={orders}
              onViewBreakdown={handleViewBreakdown}
              isLoading={isLoading}
            />

            {totalCount > 0 && (
              <OrdersPagination
                currentPage={currentPage}
                totalPages={totalPages}
                pageSize={pageSize}
                totalItems={totalCount}
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

      <TaxBreakdownModal
        open={!!selectedOrderId}
        onClose={() => setSelectedOrderId(null)}
        breakdown={breakdown}
        isLoading={isDetailLoading}
      />

      <Toaster />
    </div>
  );
}

export default App;
