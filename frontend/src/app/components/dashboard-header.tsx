import { Bell, Search } from 'lucide-react';
import { Avatar, AvatarFallback, AvatarImage } from './ui/avatar';

export function DashboardHeader() {
  return (
    <header className="h-16 border-b border-border bg-white sticky top-0 z-10">
      <div className="h-full px-8 flex items-center justify-between">
        {/* Search */}
        <div className="flex-1 max-w-md">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Search orders..."
              className="w-full pl-10 pr-4 py-2 bg-muted/50 border border-transparent rounded-lg focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-sm"
            />
          </div>
        </div>

        {/* Right Section */}
        <div className="flex items-center gap-4">
          <button className="relative p-2 hover:bg-muted/50 rounded-lg transition-colors">
            <Bell className="w-5 h-5 text-muted-foreground" />
            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-primary rounded-full" />
          </button>
          
          <div className="flex items-center gap-3 pl-4 border-l border-border">
            <div className="text-right">
              <p className="text-sm font-medium text-foreground">Admin User</p>
              <p className="text-xs text-muted-foreground">admin@wellness.com</p>
            </div>
            <Avatar className="w-9 h-9">
              <AvatarImage src="" />
              <AvatarFallback className="bg-gradient-to-br from-[#4ade80] to-[#22c55e] text-white text-sm">
                AU
              </AvatarFallback>
            </Avatar>
          </div>
        </div>
      </div>
    </header>
  );
}
