import { Package, FileText, Settings, TrendingUp, Plane } from 'lucide-react';

export function DashboardSidebar() {
  return (
    <div className="w-64 min-h-screen bg-white border-r border-border flex flex-col">
      {/* Logo and Brand */}
      <div className="p-6 border-b border-border">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#4ade80] to-[#22c55e] flex items-center justify-center shadow-lg">
            <Plane className="w-5 h-5 text-white transform hover:scale-110 transition-transform" />
          </div>
          <div>
            <h2 className="font-semibold text-[15px] text-foreground">Instant Wellness</h2>
            <p className="text-xs text-muted-foreground">Admin Portal</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4">
        <div className="space-y-1">
          <a
            href="#"
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg bg-[#f0fdf4] text-[#166534] transition-colors"
          >
            <Package className="w-5 h-5" />
            <span className="text-[15px] font-medium">Orders</span>
          </a>
          <a
            href="#"
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-muted-foreground hover:bg-muted/50 transition-colors"
          >
            <TrendingUp className="w-5 h-5" />
            <span className="text-[15px]">Analytics</span>
          </a>
          <a
            href="#"
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-muted-foreground hover:bg-muted/50 transition-colors"
          >
            <FileText className="w-5 h-5" />
            <span className="text-[15px]">Reports</span>
          </a>
          <a
            href="#"
            className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-muted-foreground hover:bg-muted/50 transition-colors"
          >
            <Settings className="w-5 h-5" />
            <span className="text-[15px]">Settings</span>
          </a>
        </div>
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-border">
        <div className="px-3 py-2 rounded-lg bg-muted/50">
          <p className="text-xs text-muted-foreground">Drone Delivery Zone</p>
          <p className="text-sm font-medium text-foreground mt-0.5">New York State</p>
        </div>
      </div>
    </div>
  );
}