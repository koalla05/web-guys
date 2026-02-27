import { useState } from 'react';
import { Plus } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { Input } from './ui/input';
import { Label } from './ui/label';

interface CreateOrderCardProps {
  onCreateOrder: (order: {
    latitude: number;
    longitude: number;
    subtotal: number;
    timestamp: string;
  }) => void;
}

export function CreateOrderCard({ onCreateOrder }: CreateOrderCardProps) {
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [subtotal, setSubtotal] = useState('');
  const [timestamp, setTimestamp] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (latitude && longitude && subtotal && timestamp) {
      onCreateOrder({
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude),
        subtotal: parseFloat(subtotal),
        timestamp,
      });
      // Reset form
      setLatitude('');
      setLongitude('');
      setSubtotal('');
      setTimestamp('');
    }
  };

  return (
    <Card className="shadow-sm">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Plus className="w-5 h-5 text-secondary" />
          Manual Create Order
        </CardTitle>
        <CardDescription>
          Create a new order with delivery coordinates
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="latitude">Latitude</Label>
              <Input
                id="latitude"
                type="number"
                step="any"
                placeholder="40.7128"
                value={latitude}
                onChange={(e) => setLatitude(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="longitude">Longitude</Label>
              <Input
                id="longitude"
                type="number"
                step="any"
                placeholder="-74.0060"
                value={longitude}
                onChange={(e) => setLongitude(e.target.value)}
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="subtotal">Subtotal ($)</Label>
            <Input
              id="subtotal"
              type="number"
              step="0.01"
              placeholder="99.99"
              value={subtotal}
              onChange={(e) => setSubtotal(e.target.value)}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="timestamp">Timestamp</Label>
            <Input
              id="timestamp"
              type="datetime-local"
              value={timestamp}
              onChange={(e) => setTimestamp(e.target.value)}
              required
            />
          </div>

          <Button type="submit" className="w-full">
            Create Order
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
