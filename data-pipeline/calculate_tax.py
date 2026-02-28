#!/usr/bin/env python3
"""
Tax calculation using tax_rates.csv (from tax_rates_retrive.py).
Accepts lat, lon, subtotal; returns JSON with tax breakdown.
Uses Nominatim for reverse geocoding (lat/lon -> county/city).
"""
import argparse
import json
import re
import sys
from pathlib import Path

import pandas as pd
import requests

SCRIPT_DIR = Path(__file__).resolve().parent
TAX_CSV = SCRIPT_DIR / "tax_rates.csv"
NOMINATIM_URL = "https://nominatim.openstreetmap.org/reverse"


def load_tax_rates():
    """Load tax rates from CSV produced by tax_rates_retrive.py."""
    if not TAX_CSV.exists():
        return None
    df = pd.read_csv(TAX_CSV)
    return df


def reverse_geocode(lat: float, lon: float) -> tuple[str | None, str | None]:
    """Reverse geocode lat/lon to (county, city) using Nominatim."""
    try:
        r = requests.get(
            NOMINATIM_URL,
            params={"lat": lat, "lon": lon, "format": "json", "addressdetails": 1},
            headers={"User-Agent": "InstantWellness/1.0 (tax calculation)"},
            timeout=10,
        )
        r.raise_for_status()
        data = r.json()
        addr = data.get("address", {})
        county = (
            addr.get("county")
            or addr.get("state_district")
            or addr.get("state")
        )
        city = addr.get("city") or addr.get("town") or addr.get("village") or addr.get("municipality")
        return (county.strip() if county else None, city.strip() if city else None)
    except Exception:
        return None, None


def normalize_county(county: str) -> str:
    """Map Nominatim county names to CSV column values."""
    if not county:
        return "New York State only"
    c = county.strip().replace(" County", "")
    mapping = {
        "kings": "Kings (Brooklyn)",
        "brooklyn": "Brooklyn",
        "new york": "New York (Manhattan)",
        "manhattan": "Manhattan",
        "richmond": "Richmond (Staten Island)",
        "staten island": "Staten Island",
    }
    return mapping.get(c.lower(), c)


def parse_special_rates(json_str) -> float:
    """Sum rates from special_rates JSON column."""
    if not json_str or json_str == "[]":
        return 0.0
    try:
        arr = json.loads(json_str) if isinstance(json_str, str) else json_str
        return sum(item.get("rate", 0) for item in arr)
    except Exception:
        return 0.0


def lookup_rate(df: pd.DataFrame, county: str, city: str | None) -> dict | None:
    """Find tax rate row for county/city. CSV uses '0' for county-level city."""
    county = normalize_county(county)
    city_val = "0" if not city or not str(city).strip() else str(city).strip()

    # Try exact match: state=NY, county, city
    mask = (
        (df["state"].str.upper() == "NY")
        & (df["county"].str.strip().str.lower() == county.lower())
        & (df["city"].astype(str).str.strip().str.lower() == city_val.lower())
    )
    row = df[mask]
    if not row.empty:
        return row.iloc[0].to_dict()

    # Fallback: county-level (city=0)
    mask = (
        (df["state"].str.upper() == "NY")
        & (df["county"].str.strip().str.lower() == county.lower())
        & (df["city"].astype(str).str.strip() == "0")
    )
    row = df[mask]
    if not row.empty:
        return row.iloc[0].to_dict()

    # Fallback: NY state only
    mask = (
        (df["state"].str.upper() == "NY")
        & (df["county"].str.strip().str.lower() == "new york state only")
        & (df["city"].astype(str).str.strip() == "0")
    )
    row = df[mask]
    if not row.empty:
        return row.iloc[0].to_dict()

    return None


def calculate_tax(lat: float, lon: float, subtotal: float) -> dict:
    """Main entry: geocode, lookup rate, compute tax. Returns JSON-serializable dict."""
    df = load_tax_rates()
    if df is None or df.empty:
        return {
            "error": "tax_rates.csv not found",
            "composite_tax_rate": 0.04,
            "tax_amount": float(subtotal) * 0.04,
            "state": "NY",
            "county": "",
            "city": "",
            "state_rate": 0.04,
            "county_rate": 0.0,
            "city_rate": 0.0,
            "special_rates": 0.0,
        }

    county, city = reverse_geocode(lat, lon)
    if not county:
        county = "New York State only"
        city = None

    row = lookup_rate(df, county, city)
    if row is None:
        row = lookup_rate(df, "New York State only", None) or {}
        county, city = "New York State only", None

    composite = float(row.get("composite_tax_rate", 0.04))
    state_rate = float(row.get("state_rate", 0.04))
    county_rate = float(row.get("county_rate", 0))
    city_rate = float(row.get("city_rate", 0))
    special_rates = parse_special_rates(row.get("special_rates", "[]"))
    tax_amount = float(subtotal) * composite

    return {
        "state": row.get("state", "NY"),
        "county": str(row.get("county", "")),
        "city": "" if str(row.get("city", "0")) == "0" else str(row.get("city", "")),
        "special_jurisdiction": row.get("special_districts") or None,
        "state_rate": state_rate,
        "county_rate": county_rate,
        "city_rate": city_rate,
        "special_rates": special_rates,
        "composite_tax_rate": composite,
        "tax_amount": round(tax_amount, 2),
    }


def main():
    # Allow "--" before args so negative longitude isn't parsed as a flag (e.g. from C# ProcessStartInfo)
    if "--" in sys.argv:
        sys.argv = [a for a in sys.argv if a != "--"]
    parser = argparse.ArgumentParser(description="Calculate NY sales tax for lat/lon and subtotal")
    parser.add_argument("lat", type=float, help="Latitude")
    parser.add_argument("lon", type=float, help="Longitude (use -- before args if negative)")
    parser.add_argument("subtotal", type=float, help="Subtotal amount")
    parser.add_argument("--pretty", action="store_true", help="Pretty-print JSON")
    args = parser.parse_args()

    result = calculate_tax(args.lat, args.lon, args.subtotal)
    kwargs = {"indent": 2} if args.pretty else {}
    print(json.dumps(result, **kwargs))


if __name__ == "__main__":
    main()
