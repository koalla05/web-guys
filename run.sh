#!/bin/bash
set -e

ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"

cleanup() {
  echo ""
  echo "Shutting down..."
  [ -n "$BACKEND_PID" ] && kill $BACKEND_PID 2>/dev/null || true
  exit 0
}
trap cleanup SIGINT SIGTERM

echo "----Starting Instant Wellness App----"
echo ""

# Install frontend deps if needed
if [ ! -d "frontend/node_modules" ]; then
  echo "Installing frontend dependencies..."
  cd frontend && npm install && cd ..
  echo ""
fi

# Start backend
echo "Starting backend (http://localhost:5046)..."
cd "$ROOT/src/InstantWellness.Api"
dotnet run &
BACKEND_PID=$!
cd "$ROOT"

echo "Waiting for backend to start..."
for i in {1..30}; do
  if curl -s -o /dev/null -w "%{http_code}" http://localhost:5046/orders 2>/dev/null | grep -qE "200|401"; then
    echo "Backend ready."
    break
  fi
  [ $i -eq 30 ] && echo "Backend failed to start in time." && kill $BACKEND_PID 2>/dev/null && exit 1
  sleep 1
done
echo ""

# Start frontend
echo "Starting frontend (http://localhost:5173)..."
echo "Press Ctrl+C to stop both."
echo ""
cd "$ROOT/frontend"
npm run dev
