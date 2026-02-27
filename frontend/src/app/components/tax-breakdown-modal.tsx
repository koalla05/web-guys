import { X } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from './ui/dialog';
import { Badge } from './ui/badge';
import { Skeleton } from './ui/skeleton';

interface TaxBreakdown {
  orderId: string;
  stateRate: number;
  countyRate: number;
  cityRate: number;
  specialRate: number;
  compositeTaxRate: number;
  taxAmount: number;
  subtotal: number;
  totalAmount: number;
  state: string;
  county: string;
  city: string;
  specialDistrict?: string;
}

interface TaxBreakdownModalProps {
  open: boolean;
  onClose: () => void;
  breakdown: TaxBreakdown | null;
  isLoading?: boolean;
}

export function TaxBreakdownModal({ open, onClose, breakdown, isLoading }: TaxBreakdownModalProps) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>Tax Breakdown Details</DialogTitle>
          <DialogDescription>
            {breakdown
              ? `Complete tax calculation for Order #${breakdown.orderId.slice(0, 8)}...`
              : 'Loading...'}
          </DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-6 mt-4">
            <Skeleton className="h-24 w-full" />
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : breakdown ? (
          <div className="space-y-6 mt-4">
            {/* Tax Rates Section - state_rate, county_rate, city_rate, special_rates */}
            <div>
              <h3 className="text-sm font-semibold text-foreground mb-3">Tax Rate Breakdown</h3>
              <div className="grid grid-cols-2 gap-4">
                <div className="p-4 rounded-lg bg-muted/30 border border-border">
                  <p className="text-xs text-muted-foreground mb-1">State Rate</p>
                  <p className="text-lg font-semibold text-foreground">{breakdown.stateRate.toFixed(2)}%</p>
                </div>
                <div className="p-4 rounded-lg bg-muted/30 border border-border">
                  <p className="text-xs text-muted-foreground mb-1">County Rate</p>
                  <p className="text-lg font-semibold text-foreground">{breakdown.countyRate.toFixed(2)}%</p>
                </div>
                <div className="p-4 rounded-lg bg-muted/30 border border-border">
                  <p className="text-xs text-muted-foreground mb-1">City Rate</p>
                  <p className="text-lg font-semibold text-foreground">{breakdown.cityRate.toFixed(2)}%</p>
                </div>
                <div className="p-4 rounded-lg bg-muted/30 border border-border">
                  <p className="text-xs text-muted-foreground mb-1">Special Rates</p>
                  <p className="text-lg font-semibold text-foreground">{breakdown.specialRate.toFixed(2)}%</p>
                </div>
              </div>
            </div>

            {/* Composite Rate */}
            <div className="p-4 rounded-lg bg-gradient-to-br from-primary/10 to-secondary/10 border-2 border-primary/20">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-muted-foreground mb-1">Composite Tax Rate</p>
                  <p className="text-2xl font-bold text-foreground">{breakdown.compositeTaxRate.toFixed(2)}%</p>
                </div>
                <Badge className="bg-primary text-primary-foreground">Combined</Badge>
              </div>
            </div>

            {/* Amount Breakdown */}
            <div>
              <h3 className="text-sm font-semibold text-foreground mb-3">Amount Calculation</h3>
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2 border-b border-border">
                  <span className="text-sm text-muted-foreground">Subtotal</span>
                  <span className="text-sm font-medium text-foreground">
                    ${breakdown.subtotal.toFixed(2)}
                  </span>
                </div>
                <div className="flex items-center justify-between py-2 border-b border-border">
                  <span className="text-sm text-muted-foreground">Tax Amount</span>
                  <span className="text-sm font-medium text-secondary">
                    ${breakdown.taxAmount.toFixed(2)}
                  </span>
                </div>
                <div className="flex items-center justify-between py-3 bg-muted/30 px-4 rounded-lg">
                  <span className="font-semibold text-foreground">Total Amount</span>
                  <span className="text-lg font-bold text-foreground">
                    ${breakdown.totalAmount.toFixed(2)}
                  </span>
                </div>
              </div>
            </div>

            {/* Jurisdictions */}
            <div>
              <h3 className="text-sm font-semibold text-foreground mb-3">Applied Jurisdictions</h3>
              <div className="grid grid-cols-2 gap-3">
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground w-20">State:</span>
                  <Badge variant="outline">{breakdown.state}</Badge>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground w-20">County:</span>
                  <Badge variant="outline">{breakdown.county}</Badge>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-muted-foreground w-20">City:</span>
                  <Badge variant="outline">{breakdown.city}</Badge>
                </div>
                {breakdown.specialDistrict && (
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground w-20">Special:</span>
                    <Badge variant="outline">{breakdown.specialDistrict}</Badge>
                  </div>
                )}
              </div>
            </div>
          </div>
        ) : null}
      </DialogContent>
    </Dialog>
  );
}
