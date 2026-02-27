import { TrendingUp, DollarSign, Package, Percent } from 'lucide-react';
import { Card, CardContent } from './ui/card';

interface StatsCardsProps {
  totalOrders: number;
  totalRevenue: number;
  totalTax: number;
  averageTaxRate: number;
}

export function StatsCards({ totalOrders, totalRevenue, totalTax, averageTaxRate }: StatsCardsProps) {
  const stats = [
    {
      label: 'Total Orders',
      value: totalOrders.toString(),
      icon: Package,
      color: 'text-primary',
      bgColor: 'bg-primary/10',
    },
    {
      label: 'Total Revenue',
      value: `$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
      icon: DollarSign,
      color: 'text-secondary',
      bgColor: 'bg-secondary/10',
    },
    {
      label: 'Total Tax Collected',
      value: `$${totalTax.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
      icon: TrendingUp,
      color: 'text-[#a78bfa]',
      bgColor: 'bg-[#a78bfa]/10',
    },
    {
      label: 'Avg Tax Rate',
      value: `${averageTaxRate.toFixed(2)}%`,
      icon: Percent,
      color: 'text-[#fb923c]',
      bgColor: 'bg-[#fb923c]/10',
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      {stats.map((stat, index) => {
        const Icon = stat.icon;
        return (
          <Card key={index} className="shadow-sm hover:shadow-md transition-shadow">
            <CardContent className="p-6">
              <div className="flex items-center justify-between mb-4">
                <div className={`w-12 h-12 rounded-xl ${stat.bgColor} flex items-center justify-center`}>
                  <Icon className={`w-6 h-6 ${stat.color}`} />
                </div>
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">{stat.label}</p>
                <p className="text-2xl font-bold text-foreground">{stat.value}</p>
              </div>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
