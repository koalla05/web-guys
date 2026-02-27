// In dev: /api is proxied to backend. In prod: use VITE_API_URL env.
const API_BASE = import.meta.env.VITE_API_URL || (import.meta.env.DEV ? '/api' : 'http://localhost:5046');

// API response types (snake_case from backend)
export interface Order {
  id: string;
  latitude: number;
  longitude: number;
  subtotal: number;
  timestamp: string;
  composite_tax_rate: number;
  tax_amount: number;
  total_amount: number;
  county?: string | null;
}

export interface PagedOrders {
  items: Order[];
  total_count: number;
  page: number;
  page_size: number;
  total_pages: number;
}

export interface OrderDetail {
  id: string;
  latitude: number;
  longitude: number;
  subtotal: number;
  timestamp: string;
  tax_rate_breakdown: {
    state_rate: number;
    county_rate: number;
    city_rate: number;
    special_rates: number;
    composite_tax_rate: number;
  };
  amount_calculation: {
    subtotal: number;
    tax_amount: number;
    total_amount: number;
  };
  applied_jurisdictions: {
    state?: string | null;
    county?: string | null;
    city?: string | null;
    special?: string | null;
  };
}

export interface ImportResult {
  imported_count: number;
  failed_count: number;
  errors: string[];
}

export interface ApiError {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const err: ApiError = await res.json().catch(() => ({}));
    throw new Error(err.detail || err.title || `Request failed: ${res.status}`);
  }
  return res.json();
}

export async function getOrders(params: {
  page?: number;
  pageSize?: number;
  orderIdSearch?: string;
  fromDate?: string;
  toDate?: string;
  county?: string;
  minAmount?: number;
  maxAmount?: number;
  minTaxRate?: number;
  maxTaxRate?: number;
}): Promise<PagedOrders> {
  const search = new URLSearchParams();
  if (params.page != null) search.set('page', String(params.page));
  if (params.pageSize != null) search.set('pageSize', String(params.pageSize));
  if (params.orderIdSearch) search.set('orderIdSearch', params.orderIdSearch);
  if (params.fromDate) search.set('fromDate', params.fromDate);
  if (params.toDate) search.set('toDate', params.toDate);
  if (params.county) search.set('county', params.county);
  if (params.minAmount != null) search.set('minAmount', String(params.minAmount));
  if (params.maxAmount != null) search.set('maxAmount', String(params.maxAmount));
  if (params.minTaxRate != null) search.set('minTaxRate', String(params.minTaxRate));
  if (params.maxTaxRate != null) search.set('maxTaxRate', String(params.maxTaxRate));

  const res = await fetch(`${API_BASE}/orders?${search}`);
  return handleResponse<PagedOrders>(res);
}

export async function getOrderById(id: string): Promise<OrderDetail | null> {
  const res = await fetch(`${API_BASE}/orders/${id}`);
  if (res.status === 404) return null;
  return handleResponse<OrderDetail>(res);
}

export async function createOrder(data: {
  latitude: number;
  longitude: number;
  subtotal: number;
  timestamp?: string;
}): Promise<Order> {
  const body = {
    latitude: data.latitude,
    longitude: data.longitude,
    subtotal: data.subtotal,
    timestamp: data.timestamp || null,
  };
  const res = await fetch(`${API_BASE}/orders`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  return handleResponse<Order>(res);
}

export async function importOrders(file: File): Promise<ImportResult> {
  const form = new FormData();
  form.append('file', file);
  const res = await fetch(`${API_BASE}/orders/import`, {
    method: 'POST',
    body: form,
  });
  return handleResponse<ImportResult>(res);
}
