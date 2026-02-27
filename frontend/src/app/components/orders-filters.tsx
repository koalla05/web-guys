import { Filter, X, Search } from 'lucide-react';
import { Input } from './ui/input';
import { Label } from './ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './ui/select';
import { Card } from './ui/card';
import { Button } from './ui/button';
import { useState, useEffect } from 'react';

export interface OrderFilters {
  searchQuery?: string;
  dateFrom?: string;
  dateTo?: string;
  taxRateMin?: string;
  taxRateMax?: string;
  amountMin?: string;
  amountMax?: string;
  county?: string;
}

interface OrdersFiltersProps {
  filters: OrderFilters;
  onFilterChange: (filters: OrderFilters) => void;
  onClearFilters: () => void;
  resultsCount?: number;
}

export function OrdersFilters({ 
  filters, 
  onFilterChange, 
  onClearFilters,
  resultsCount 
}: OrdersFiltersProps) {
  // Local state for filter inputs before applying
  const [localFilters, setLocalFilters] = useState<OrderFilters>(filters);
  const [hasUnappliedChanges, setHasUnappliedChanges] = useState(false);

  // Update local filters when external filters change (e.g., clear all)
  useEffect(() => {
    setLocalFilters(filters);
    setHasUnappliedChanges(false);
  }, [filters]);

  const handleLocalFilterUpdate = (update: Partial<OrderFilters>) => {
    const newLocalFilters = { ...localFilters, ...update };
    setLocalFilters(newLocalFilters);
    
    // Check if there are differences between local and applied filters
    const hasChanges = JSON.stringify(newLocalFilters) !== JSON.stringify(filters);
    setHasUnappliedChanges(hasChanges);
  };

  const handleApplyFilters = () => {
    onFilterChange(localFilters);
    setHasUnappliedChanges(false);
  };

  const handleClearAll = () => {
    setLocalFilters({});
    onClearFilters();
    setHasUnappliedChanges(false);
  };

  const hasActiveFilters = Object.values(filters).some(value => value && value !== 'all');

  return (
    <Card className="p-4 shadow-sm">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <Filter className="w-4 h-4 text-primary" />
          <h3 className="font-semibold text-foreground">Filters</h3>
          {resultsCount !== undefined && (
            <span className="text-xs text-muted-foreground">
              ({resultsCount} result{resultsCount !== 1 ? 's' : ''})
            </span>
          )}
          {hasUnappliedChanges && (
            <span className="text-xs text-amber-600 font-medium">
              • Unapplied changes
            </span>
          )}
        </div>
        <div className="flex items-center gap-2">
          {hasActiveFilters && (
            <Button
              variant="ghost"
              size="sm"
              onClick={handleClearAll}
              className="text-muted-foreground hover:text-foreground"
            >
              <X className="w-4 h-4 mr-1" />
              Clear all
            </Button>
          )}
        </div>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-4">
        {/* Search by Order ID */}
        <div className="space-y-2 col-span-1 md:col-span-2 lg:col-span-1 xl:col-span-2">
          <Label htmlFor="search" className="text-xs">Search Order ID</Label>
          <Input
            id="search"
            type="text"
            placeholder="e.g., ORD-00001"
            value={localFilters.searchQuery || ''}
            onChange={(e) => handleLocalFilterUpdate({ searchQuery: e.target.value })}
            className="text-sm"
          />
        </div>

        {/* Date From */}
        <div className="space-y-2">
          <Label htmlFor="date-from" className="text-xs">Date From</Label>
          <Input
            id="date-from"
            type="date"
            value={localFilters.dateFrom || ''}
            onChange={(e) => handleLocalFilterUpdate({ dateFrom: e.target.value })}
            className="text-sm"
          />
        </div>

        {/* Date To */}
        <div className="space-y-2">
          <Label htmlFor="date-to" className="text-xs">Date To</Label>
          <Input
            id="date-to"
            type="date"
            value={localFilters.dateTo || ''}
            onChange={(e) => handleLocalFilterUpdate({ dateTo: e.target.value })}
            className="text-sm"
          />
        </div>

        {/* County Filter */}
        <div className="space-y-2">
          <Label htmlFor="county" className="text-xs">County</Label>
          <Select 
            value={localFilters.county || 'all'} 
            onValueChange={(value) => handleLocalFilterUpdate({ county: value === 'all' ? undefined : value })}
          >
            <SelectTrigger id="county" className="text-sm">
              <SelectValue placeholder="All Counties" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Counties</SelectItem>
              <SelectItem value="New York">New York</SelectItem>
              <SelectItem value="Kings">Kings</SelectItem>
              <SelectItem value="Queens">Queens</SelectItem>
              <SelectItem value="Nassau">Nassau</SelectItem>
              <SelectItem value="Suffolk">Suffolk</SelectItem>
              <SelectItem value="Westchester">Westchester</SelectItem>
              <SelectItem value="Erie">Erie</SelectItem>
              <SelectItem value="Monroe">Monroe</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {/* Min Amount */}
        <div className="space-y-2">
          <Label htmlFor="amount-min" className="text-xs">Min Amount ($)</Label>
          <Input
            id="amount-min"
            type="number"
            step="0.01"
            placeholder="0.00"
            value={localFilters.amountMin || ''}
            onChange={(e) => handleLocalFilterUpdate({ amountMin: e.target.value })}
            className="text-sm"
          />
        </div>

        {/* Max Amount */}
        <div className="space-y-2">
          <Label htmlFor="amount-max" className="text-xs">Max Amount ($)</Label>
          <Input
            id="amount-max"
            type="number"
            step="0.01"
            placeholder="1000.00"
            value={localFilters.amountMax || ''}
            onChange={(e) => handleLocalFilterUpdate({ amountMax: e.target.value })}
            className="text-sm"
          />
        </div>
      </div>

      {/* Secondary row for tax rate filters */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 xl:grid-cols-7 gap-4 mt-4">
        <div className="space-y-2">
          <Label htmlFor="tax-min" className="text-xs">Min Tax Rate (%)</Label>
          <Input
            id="tax-min"
            type="number"
            step="0.1"
            placeholder="0"
            value={localFilters.taxRateMin || ''}
            onChange={(e) => handleLocalFilterUpdate({ taxRateMin: e.target.value })}
            className="text-sm"
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="tax-max" className="text-xs">Max Tax Rate (%)</Label>
          <Input
            id="tax-max"
            type="number"
            step="0.1"
            placeholder="20"
            value={localFilters.taxRateMax || ''}
            onChange={(e) => handleLocalFilterUpdate({ taxRateMax: e.target.value })}
            className="text-sm"
          />
        </div>

        {/* Apply Filters Button */}
        <div className="space-y-2 flex items-end col-span-1 md:col-span-2 lg:col-span-2 xl:col-span-2">
          <Button 
            onClick={handleApplyFilters}
            className="w-full bg-primary hover:bg-primary/90 text-primary-foreground"
            disabled={!hasUnappliedChanges}
          >
            <Search className="w-4 h-4 mr-2" />
            Apply Filters
          </Button>
        </div>
      </div>
    </Card>
  );
}