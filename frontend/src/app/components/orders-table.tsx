import { Eye } from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import { Button } from './ui/button';
import { Badge } from './ui/badge';
import { Skeleton } from './ui/skeleton';

export interface Order {
  id: string;
  timestamp: string;
  latitude: number;
  longitude: number;
  subtotal: number;
  compositeTaxRate: number;
  taxAmount: number;
  totalAmount: number;
  county: string;
}

interface OrdersTableProps {
  orders: Order[];
  onViewBreakdown: (orderId: string) => void;
  isLoading?: boolean;
}

export function OrdersTable({ orders, onViewBreakdown, isLoading }: OrdersTableProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    );
  }

  if (orders.length === 0) {
    return (
      <div className="text-center py-16 px-4">
        <div className="w-16 h-16 rounded-full bg-muted/50 flex items-center justify-center mx-auto mb-4">
          <Eye className="w-8 h-8 text-muted-foreground" />
        </div>
        <h3 className="text-lg font-semibold text-foreground mb-2">No orders found</h3>
        <p className="text-sm text-muted-foreground max-w-sm mx-auto">
          Get started by uploading a CSV file or creating a new order manually.
        </p>
      </div>
    );
  }

  return (
    <div className="border border-border rounded-lg overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/30">
            <TableHead className="font-semibold">Order ID</TableHead>
            <TableHead className="font-semibold">Timestamp</TableHead>
            <TableHead className="font-semibold">Latitude</TableHead>
            <TableHead className="font-semibold">Longitude</TableHead>
            <TableHead className="font-semibold text-right">Subtotal</TableHead>
            <TableHead className="font-semibold text-right">Tax Rate</TableHead>
            <TableHead className="font-semibold text-right">Tax Amount</TableHead>
            <TableHead className="font-semibold text-right">Total</TableHead>
            <TableHead className="font-semibold text-center">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {orders.map((order) => (
            <TableRow key={order.id} className="hover:bg-muted/20">
              <TableCell className="font-medium">
                <Badge variant="outline" className="font-mono text-xs">
                  {order.id}
                </Badge>
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {new Date(order.timestamp).toLocaleString('en-US', {
                  month: 'short',
                  day: 'numeric',
                  year: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </TableCell>
              <TableCell className="text-sm font-mono">{order.latitude.toFixed(4)}</TableCell>
              <TableCell className="text-sm font-mono">{order.longitude.toFixed(4)}</TableCell>
              <TableCell className="text-right font-medium">
                ${order.subtotal.toFixed(2)}
              </TableCell>
              <TableCell className="text-right">
                <Badge className="bg-primary/10 text-primary border-primary/20">
                  {order.compositeTaxRate}%
                </Badge>
              </TableCell>
              <TableCell className="text-right font-medium text-secondary">
                ${order.taxAmount.toFixed(2)}
              </TableCell>
              <TableCell className="text-right font-bold">
                ${order.totalAmount.toFixed(2)}
              </TableCell>
              <TableCell className="text-center">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onViewBreakdown(order.id)}
                  className="hover:bg-primary/10 hover:text-primary"
                >
                  <Eye className="w-4 h-4 mr-2" />
                  View
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
