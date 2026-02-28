import { Plane } from 'lucide-react';

export function PageHeader() {
  return (
    <div className="mb-8">
      <div className="flex items-center gap-3 mb-2">
        <div className="relative">
          <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-primary to-primary/70 flex items-center justify-center shadow-md">
            <Plane className="w-5 h-5 text-white" />
          </div>
          <div className="absolute -top-1 -right-1 w-3 h-3 bg-secondary rounded-full animate-pulse" />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-foreground">Orders Management</h1>
          <p className="text-sm text-muted-foreground">
            Manage and calculate composite sales tax for drone deliveries
          </p>
        </div>
      </div>
    </div>
  );
}
